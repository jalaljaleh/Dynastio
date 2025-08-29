using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Configuration for the badge‐to‐role bridge feature.
    /// Controls whether the bridge is active and
    /// how role names are generated from badge identifiers.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class BadgeBridgeSettings
    {
        // persisted fields ---------------------------------------------------

        /// <summary>
        /// Toggle for enabling or disabling the badge bridge.
        /// </summary>
        [BsonElement("enabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Prefix applied to each generated Discord role name.
        /// Example: if set to "Badge_", then a badge "Gold" → "Badge_Gold".
        /// Must not be null or empty.
        /// </summary>
        [BsonElement("rolePrefix")]
        public string BadgeRoleAssignmentPrefix { get; set; } = "badge: ";


        // constructors & defaults --------------------------------------------

        /// <summary>
        /// Creates a new settings instance with default values:
        /// disabled bridge and "Badge_" prefix.
        /// </summary>
        public BadgeBridgeSettings() { }

        /// <summary>
        /// Returns a fresh default instance.
        /// </summary>
        public static BadgeBridgeSettings Default()
            => new BadgeBridgeSettings
            {
                IsEnabled = false,
                BadgeRoleAssignmentPrefix = "badge: "
            };


        // validation & cloning -----------------------------------------------

        /// <summary>
        /// Ensures all required properties are valid,
        /// throwing an exception if not.
        /// </summary>
        public void Validate()
        {
            if (BadgeRoleAssignmentPrefix is null)
                throw new InvalidOperationException("BadgeRoleAssignmentPrefix must not be null.");

            if (BadgeRoleAssignmentPrefix.Length == 0)
                throw new InvalidOperationException("BadgeRoleAssignmentPrefix must not be empty.");
        }

        /// <summary>
        /// Returns a deep copy of this settings object.
        /// </summary>
        public BadgeBridgeSettings Clone()
            => new BadgeBridgeSettings
            {
                IsEnabled = this.IsEnabled,
                BadgeRoleAssignmentPrefix = this.BadgeRoleAssignmentPrefix
            };


        // utility methods ----------------------------------------------------

        /// <summary>
        /// Generates a Discord role name from a badge name,
        /// applying the configured prefix and sanitizing whitespace.
        /// </summary>
        /// <param name="badgeName">The raw badge identifier (e.g. "Gold Member").</param>
        /// <returns>
        /// A sanitized role name (e.g. "Badge_Gold_Member").
        /// </returns>
        public string BuildRoleName(string badgeName)
        {
            if (string.IsNullOrWhiteSpace(badgeName))
                throw new ArgumentException("badgeName cannot be null or whitespace.", nameof(badgeName));

            // replace internal spaces with underscores, trim edges
            var sanitized = badgeName
                .Trim()
                .Replace(' ', '_');

            return $"{BadgeRoleAssignmentPrefix}{sanitized}";
        }
    }
}
