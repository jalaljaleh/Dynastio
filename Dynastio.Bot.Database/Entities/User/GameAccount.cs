using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// A linked game account (e.g. Steam, PSN) for a <see cref="User"/>.
    /// Immutable identifier comprised of ServiceName + Delimiter + ServiceAccountId.
    /// Fluent setters allow updating individual fields while preserving invariants.
    /// </summary>
    [BsonIgnoreExtraElements]
    public sealed class GameAccount 
    {
        [JsonIgnore]
        private const char Delimiter = ':';
        [JsonIgnore]
        private const int MaxDisplayNameLength = 16;

        //======================================================================
        // Persistent fields (BSON)
        //======================================================================

        /// <summary>
        /// Composite key: "{ServiceName}:{ServiceAccountId}".
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.String)]
        [JsonInclude, JsonPropertyName("_id")]

        public string Id { get; private set; }

        /// <summary>
        /// The service name (Steam, PSN, etc.).
        /// </summary>
        [JsonInclude, JsonPropertyName("service")]
        [BsonElement("service")]
        public string ServiceName { get; private set; }

        /// <summary>
        /// The opaque account identifier assigned by the service.
        /// </summary>
        [JsonInclude, JsonPropertyName("accountId")]
        [BsonElement("accountId")]
        public string ServiceAccountId { get; private set; }

        /// <summary>
        /// One-time PIN code used for account verification (if any).
        /// </summary>
        [JsonInclude, JsonPropertyName("pinCode")]
        [BsonElement("pinCode")]
        public string PinCode { get; private set; }

        /// <summary>
        /// Contact email for notifications related to this account.
        /// </summary>
        [JsonInclude, JsonPropertyName("email")]
        [BsonElement("email")]
        public string Email { get; private set; }

        /// <summary>
        /// When this account was first linked (UTC).
        /// </summary>
        [JsonInclude, JsonPropertyName("linkedAtUtc")]
        [BsonElement("linkedAtUtc"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LinkedAtUtc { get; private set; }

        /// <summary>
        /// If true, this is the default account for lookups.
        /// </summary>
        [JsonInclude, JsonPropertyName("isDefault")]
        [BsonElement("isDefault")]
        public bool IsDefault { get; private set; }

        /// <summary>
        /// A short label or reminder (<see cref="MaxDisplayNameLength"/> chars max).
        /// </summary>
        [JsonInclude, JsonPropertyName("displayName")]
        [BsonElement("displayName")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// A short label or reminder 200 chars max).
        /// </summary>
        [JsonInclude, JsonPropertyName("notes")]
        [BsonElement("notes")]
        public string Notes { get; private set; }
        //======================================================================
        // Computed / ignored fields
        //======================================================================

        /// <summary>
        /// The composite ID split by the delimiter.
        /// </summary>
        [BsonIgnore]
        [JsonIgnore]
        public (string service, string accountId) ParsedId
            => (ServiceName, ServiceAccountId);


        //======================================================================
        // Construction & Parsing
        //======================================================================

      //  private GameAccount() { /* for deserialization */ }

        /// <summary>
        /// Creates a new <see cref="GameAccount"/> from service + accountId.
        /// </summary>
        public static GameAccount Create(string Id)
        {
            string serviceName = Id.Split(":")[0], serviceAccountId = Id.Split(":")[1];

            ValidateServiceName(serviceName);
            ValidateServiceAccountId(serviceAccountId);

            return new GameAccount
            {
                ServiceName = serviceName,
                ServiceAccountId = serviceAccountId,
                Id = $"{serviceName}{Delimiter}{serviceAccountId}",
                PinCode = string.Empty,
                Email = string.Empty,
                LinkedAtUtc = DateTime.UtcNow,
                IsDefault = false,
                DisplayName = serviceName,
                Notes = string.Empty
            };
        }


        //======================================================================
        // Fluent Updaters
        //======================================================================

        /// <summary>
        /// Sets or clears the one-time PIN code.
        /// </summary>
        public GameAccount WithPin(string pin)
        {
            PinCode = pin?.Trim() ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets or clears the notification email.
        /// </summary>
        public GameAccount WithEmail(string email)
        {
            Email = email?.Trim() ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Toggles this account as default.
        /// </summary>
        public GameAccount AsDefault(bool isDefault = true)
        {
            IsDefault = isDefault;
            return this;
        }

        /// <summary>
        /// Refreshes the linked timestamp to now (UTC).
        /// </summary>
        public GameAccount TouchLinkTime()
        {
            LinkedAtUtc = DateTime.UtcNow;
            return this;
        }

        /// <summary>
        /// Sets a reminder label, truncated to <see cref="MaxDisplayNameLength"/>.
        /// </summary>
        public GameAccount WithDisplayName(string text)
        {
            var reminder = string.IsNullOrWhiteSpace(text)
                ? ServiceName
                : text.Trim().Substring(0, Math.Min(text.Length, MaxDisplayNameLength));

            DisplayName = reminder;
            return this;
        }

        /// <summary>
        /// Sets a reminder label, truncated to 200.
        /// </summary>
        public GameAccount WithNote(string text)
        {
            var reminder = string.IsNullOrWhiteSpace(text)
                ? ServiceName
                : text.Trim().Substring(0, Math.Min(text.Length, 200));

            Notes = reminder;
            return this;
        }
        /// <summary>
        /// Clears any existing reminder.
        /// </summary>
        public GameAccount ClearDisplayName()
        {
            DisplayName = string.Empty;
            return this;
        }


        //======================================================================
        // Validation
        //======================================================================

        private static void ValidateServiceName(string svc)
        {
            if (string.IsNullOrWhiteSpace(svc))
                throw new ArgumentException("ServiceName must be non-empty.", nameof(svc));
            if (svc.Contains(Delimiter))
                throw new ArgumentException($"ServiceName cannot contain '{Delimiter}'.", nameof(svc));
        }

        private static void ValidateServiceAccountId(string aid)
        {
            if (string.IsNullOrWhiteSpace(aid))
                throw new ArgumentException("ServiceAccountId must be non-empty.", nameof(aid));
            if (aid.Contains(Delimiter))
                throw new ArgumentException($"ServiceAccountId cannot contain '{Delimiter}'.", nameof(aid));
        }


        //======================================================================
        // Equality & Hashing
        //======================================================================

        public bool Equals(GameAccount other)
            => other != null && Id == other.Id;

        public override bool Equals(object obj)
            => Equals(obj as GameAccount);

        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal);

        public override string ToString()
            => Id;


        //======================================================================
        // Cloning
        //======================================================================

        /// <summary>
        /// Creates a shallow copy. All strings and primitives are copied;
        /// no deep cloning required for this POCO.
        /// </summary>
        public GameAccount Clone()
            => (GameAccount)MemberwiseClone();
    }
}
