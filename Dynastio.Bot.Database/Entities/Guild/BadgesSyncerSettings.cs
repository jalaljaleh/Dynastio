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
    public class BadgesSyncerSettings
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
        public string Prefix { get; set; } = "badge: ";


        // constructors & defaults --------------------------------------------

        /// <summary>
        /// Creates a new settings instance with default values:
        /// disabled bridge and "Badge_" prefix.
        /// </summary>
        public BadgesSyncerSettings() { }

    }
}
