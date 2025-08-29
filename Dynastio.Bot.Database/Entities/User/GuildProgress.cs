using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Tracks XP and level progress for a single Discord guild.
    /// Provides utilities to record message timestamps, add XP,
    /// and compute level according to a progression formula.
    /// </summary>
    [BsonIgnoreExtraElements]
    public sealed class GuildProgress
    {
        // persisted fields ---------------------------------------------------

        /// <summary>
        /// Discord Guild ID (stored as string in BSON to avoid overflow).
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.String), BsonElement("guildId")]
        public ulong GuildId { get; private set; }

        /// <summary>
        /// Total experience points accumulated.
        /// </summary>
        [JsonInclude, JsonPropertyName("xp")]
        [BsonElement("xp")]
        public long Xp { get; set; }

        /// <summary>
        /// Current computed level.
        /// </summary>
        [JsonInclude, JsonPropertyName("level")]
        [BsonElement("level")]
        public int Level { get; set; }

        /// <summary>
        /// UTC timestamp of the last message recorded in this guild.
        /// </summary>
        [JsonInclude, JsonPropertyName("lastMessageAt")]
        [BsonElement("lastMessageAt"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastMessageAtUtc { get; private set; }

        /// <summary>
        /// UTC timestamp of the last time XP was awarded or checked.
        /// </summary>
        [JsonInclude, JsonPropertyName("lastXpAt")]
        [BsonElement("lastXpAt"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastXpUpdateAtUtc { get; private set; }


        // constructors --------------------------------------------------------

        /// <summary>
        /// Creates a fresh progress record for the specified guild.
        /// </summary>
        public GuildProgress(ulong guildId)
        {
            GuildId = guildId;
            Xp = 0;
            Level = 0;
            LastMessageAtUtc = DateTime.MinValue;
            LastXpUpdateAtUtc = DateTime.MinValue;
        }

        /// <summary>
        /// Parameterless constructor for deserialization only.
        /// </summary>
        private GuildProgress() { }


        // behavior methods ---------------------------------------------------

        /// <summary>
        /// Records a new message timestamp for this guild.
        /// </summary>
        /// <param name="timestamp">The time of the message (any kind). Will be converted to UTC.</param>
        /// <returns>Self for fluent chaining.</returns>
        public GuildProgress RecordMessage(DateTime timestamp)
        {
            LastMessageAtUtc = timestamp.ToUniversalTime();
            return this;
        }



    }
}
