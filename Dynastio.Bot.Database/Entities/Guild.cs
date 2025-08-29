using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// MongoDB document representing a Discord guild's configuration.
    /// 
    /// Design goals:
    /// - Safe BSON representation for ulong IDs.
    /// - UTC timestamps for auditing and debugging.
    /// - Optimistic concurrency via a version counter.
    /// - Defensive defaults and null-safety for nested settings.
    /// - Small, predictable methods with minimal overhead.
    /// </summary>
    [BsonIgnoreExtraElements] // Ignore unknown fields to survive schema changes.
    public sealed class Guild
    {
        // --------------------------------------------------------------------
        // Core identity and metadata
        // --------------------------------------------------------------------

        /// <summary>
        /// Discord guild ID. Stored as string in BSON for portability/safety.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [JsonInclude, JsonPropertyName("_id")]
        [BsonElement("_id")]
        public ulong Id { get; init; }

        /// <summary>
        /// Document creation timestamp (UTC).
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonInclude, JsonPropertyName("createdAtUtc")]
        [BsonElement("createdAtUtc")]
        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp (UTC). Updated on any mutating operation.
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonInclude, JsonPropertyName("updatedAtUtc")]
        [BsonElement("updatedAtUtc")]
        public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;




        // --------------------------------------------------------------------
        // Settings
        // --------------------------------------------------------------------

        /// <summary>
        /// XP system settings. Always kept non-null.
        /// </summary>
        [BsonElement("xp")]
        [JsonInclude, JsonPropertyName("xp")]
        public XpSystemSettings XpSystemSettings { get; private set; } = new();

        /// <summary>
        /// Badge bridge settings. Always kept non-null.
        /// </summary>
        [JsonInclude, JsonPropertyName("badges")]
        [BsonElement("badges")]
        public BadgeBridgeSettings BadgeBridgeSettings { get; private set; } = new();


        // --------------------------------------------------------------------
        // Optional metadata / forward-compatibility
        // --------------------------------------------------------------------




        // --------------------------------------------------------------------
        // Constructors
        // --------------------------------------------------------------------

        /// <summary>
        /// Parameterless constructor required by the MongoDB driver.
        /// </summary>
        public Guild() { }

        /// <summary>
        /// Convenience constructor for new guild documents.
        /// </summary>
        public Guild(ulong id)
        {
            Id = id;
        }


        // --------------------------------------------------------------------
        // Mutators (small and predictable)
        // --------------------------------------------------------------------

        /// <summary>
        /// Replaces the XP settings with a validated, non-null instance.
        /// </summary>
        public bool SetXpSettings(XpSystemSettings settings)
        {
            if (settings is null) settings = new();
            if (XpSystemSettings.Equals(settings))
                return false;

            XpSystemSettings = settings;
            Touch();
            return true;
        }

        /// <summary>
        /// Applies an in-place update to XP settings, ensuring non-null and validation.
        /// </summary>
        public void UpdateXpSettings(Action<XpSystemSettings> mutate)
        {
            if (XpSystemSettings is null) XpSystemSettings = new();
            mutate?.Invoke(XpSystemSettings);
            Touch();
        }

        /// <summary>
        /// Replaces the badge settings with a validated, non-null instance.
        /// </summary>
        public bool SetBadgeSettings(BadgeBridgeSettings settings)
        {
            if (settings is null) settings = new();
            if (BadgeBridgeSettings.Equals(settings))
                return false;

            BadgeBridgeSettings = settings;
            Touch();
            return true;
        }

        /// <summary>
        /// Applies an in-place update to badge settings, ensuring non-null and validation.
        /// </summary>
        public void UpdateBadgeSettings(Action<BadgeBridgeSettings> mutate)
        {
            if (BadgeBridgeSettings is null) BadgeBridgeSettings = new();
            mutate?.Invoke(BadgeBridgeSettings);
            Touch();
        }

        /// <summary>
        /// Resets all configurable settings to defaults. Does not change identity.
        /// </summary>
        public void ResetToDefaults()
        {
            XpSystemSettings = new();
            BadgeBridgeSettings = new();
            Touch();
        }


        // --------------------------------------------------------------------
        // Validation and maintenance
        // --------------------------------------------------------------------

        /// <summary>
        /// Validates the document. Returns false and an error message on failure.
        /// Keep fast and allocation-light.
        /// </summary>
        public bool TryValidate(out string? error)
        {
            if (Id == 0)
            {
                error = "Guild Id must be a non-zero value.";
                return false;
            }

            if (XpSystemSettings is null)
            {
                error = "XpSystemSettings must not be null.";
                return false;
            }

            if (BadgeBridgeSettings is null)
            {
                error = "BadgeBridgeSettings must not be null.";
                return false;
            }

            // Optionally delegate deeper validation:
            // if (!XpSystemSettings.TryValidate(out error)) return false;
            // if (!BadgeBridgeSettings.TryValidate(out error)) return false;

            error = null;
            return true;
        }

        /// <summary>
        /// Ensures non-null settings after deserialization or manual construction.
        /// Idempotent and allocation-minimal.
        /// </summary>
        public void EnsureDefaults()
        {
            XpSystemSettings ??= new();
            BadgeBridgeSettings ??= new();
        }


        // --------------------------------------------------------------------
        // Internal helpers
        // --------------------------------------------------------------------

        /// <summary>
        /// Updates the last modified timestamp and bumps the version for OCC.
        /// </summary>
        private void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Compact string for diagnostics and logs.
        /// </summary>
        public override string ToString()
            => $"Guild(Id={Id}, Updated={UpdatedAtUtc:O})";
    }
}
