using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Configuration for the XP system:
    /// awards per‐message XP, cooldowns, boosters, logging,
    /// and auto‐role assignments as users level up.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class RankingSettings
    {
        // persisted fields ---------------------------------------------------

        /// <summary>
        /// Master switch to turn the XP system on or off.
        /// </summary>
        [BsonElement("enabled")]
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// If true, grants in‐game rewards when XP is earned.
        /// </summary>
        [BsonElement("gameRewardEnabled")]
        public bool IsGameRewardEnabled { get; set; } = false;

        /// <summary>
        /// If true, automatically assigns ranking roles upon leveling.
        /// </summary>
        [BsonElement("rankingRoleAssignmentEnabled")]
        public bool IsRankingRoleAssignmentEnabled { get; set; } = false;

        /// <summary>
        /// Prefix for generated ranking roles (e.g. "Rank_").
        /// </summary>
        [BsonElement("rankingRolePrefix")]
        public string Prefix { get; set; } = "rank: ";

        /// <summary>
        /// Base XP awarded for each valid message.
        /// </summary>
        [BsonElement("xpPerMessage")]
        public int BaseXpPerMessage { get; set; } = 90;

        /// <summary>
        /// Flat XP bonus applied if a booster is active.
        /// </summary>
        [BsonElement("boosterXp")]
        public int BoosterXp { get; set; } = 30;

        /// <summary>
        /// Maximum random XP bonus to add (0..RandomXpBonus).
        /// </summary>
        [BsonElement("randomXpBonus")]
        public int RandomXpBonus { get; set; } = 20;

        /// <summary>
        /// Cooldown (in seconds) before the same user can earn XP again.
        /// </summary>
        [BsonElement("messageCooldownSeconds")]
        public int MessageScoreCooldownSeconds { get; set; } = 40;

        /// <summary>
        /// Discord channel ID where XP events and level‐ups get logged.
        /// </summary>
        [BsonElement("rankingLogChannelId"), BsonRepresentation(BsonType.String)]
        public ulong RankingLogChannelId { get; set; }

        /// <summary>
        /// IDs of channels where XP may be earned.
        /// </summary>
        [BsonElement("allowedXpChannels"), BsonRepresentation(BsonType.String)]
        public List<ulong> AllowedXpChannelIds { get; set; } = new();


        // factory & cloning ---------------------------------------------------


        /// <summary>
        /// Creates a deep copy of these settings.
        /// </summary>
        public RankingSettings Clone() => new RankingSettings
        {
            IsEnabled = this.IsEnabled,
            IsGameRewardEnabled = this.IsGameRewardEnabled,
            IsRankingRoleAssignmentEnabled = this.IsRankingRoleAssignmentEnabled,
            Prefix = this.Prefix,
            BaseXpPerMessage = this.BaseXpPerMessage,
            BoosterXp = this.BoosterXp,
            RandomXpBonus = this.RandomXpBonus,
            MessageScoreCooldownSeconds = this.MessageScoreCooldownSeconds,
            RankingLogChannelId = this.RankingLogChannelId,
            AllowedXpChannelIds = new List<ulong>(this.AllowedXpChannelIds)
        };




        // channel utilities ---------------------------------------------------

        /// <summary>
        /// Returns true if the given channel ID is allowed for XP.
        /// </summary>
        public bool IsAllowedChannel(ulong channelId)
            => AllowedXpChannelIds.Contains(channelId);

        /// <summary>
        /// Adds a channel to the allowed list (no duplicates).
        /// </summary>
        public void AddAllowedChannel(ulong channelId)
        {
            if (!AllowedXpChannelIds.Contains(channelId))
                AllowedXpChannelIds.Add(channelId);
        }

        /// <summary>
        /// Removes a channel from the allowed list.
        /// </summary>
        public bool RemoveAllowedChannel(ulong channelId)
            => AllowedXpChannelIds.Remove(channelId);

        /// <summary>
        /// Clears all allowed‐XP channels.
        /// </summary>
        public void ClearAllowedChannels()
            => AllowedXpChannelIds.Clear();


    }
}
