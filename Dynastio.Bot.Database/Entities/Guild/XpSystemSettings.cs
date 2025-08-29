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
    public class XpSystemSettings
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
        public string RankingRolePrefix { get; set; } = "Rank_";

        /// <summary>
        /// Base XP awarded for each valid message.
        /// </summary>
        [BsonElement("xpPerMessage")]
        public int BaseXpPerMessage { get; set; } = 10;

        /// <summary>
        /// Flat XP bonus applied if a booster is active.
        /// </summary>
        [BsonElement("boosterXp")]
        public int BoosterXp { get; set; } = 0;

        /// <summary>
        /// Maximum random XP bonus to add (0..RandomXpBonus).
        /// </summary>
        [BsonElement("randomXpBonus")]
        public int RandomXpBonus { get; set; } = 5;

        /// <summary>
        /// Cooldown (in seconds) before the same user can earn XP again.
        /// </summary>
        [BsonElement("messageCooldownSeconds")]
        public int MessageScoreCooldownSeconds { get; set; } = 60;

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
        /// Returns a default settings instance with sensible defaults:
        /// XP on, base 10xp/message, 5 random bonus, 60s cooldown.
        /// </summary>
        public static XpSystemSettings Default() => new XpSystemSettings
        {
            IsEnabled = true,
            BaseXpPerMessage = 10,
            BoosterXp = 0,
            RandomXpBonus = 5,
            MessageScoreCooldownSeconds = 60,
            RankingRolePrefix = "Rank_"
        };

        /// <summary>
        /// Creates a deep copy of these settings.
        /// </summary>
        public XpSystemSettings Clone() => new XpSystemSettings
        {
            IsEnabled = this.IsEnabled,
            IsGameRewardEnabled = this.IsGameRewardEnabled,
            IsRankingRoleAssignmentEnabled = this.IsRankingRoleAssignmentEnabled,
            RankingRolePrefix = this.RankingRolePrefix,
            BaseXpPerMessage = this.BaseXpPerMessage,
            BoosterXp = this.BoosterXp,
            RandomXpBonus = this.RandomXpBonus,
            MessageScoreCooldownSeconds = this.MessageScoreCooldownSeconds,
            RankingLogChannelId = this.RankingLogChannelId,
            AllowedXpChannelIds = new List<ulong>(this.AllowedXpChannelIds)
        };


        // validation -----------------------------------------------------------

        /// <summary>
        /// Throws if any setting is out of a valid range or required fields are blank.
        /// </summary>
        public void Validate()
        {
            if (BaseXpPerMessage < 0)
                throw new InvalidOperationException("BaseXpPerMessage must be non-negative.");

            if (BoosterXp < 0)
                throw new InvalidOperationException("BoosterXp must be non-negative.");

            if (RandomXpBonus < 0)
                throw new InvalidOperationException("RandomXpBonus must be non-negative.");

            if (MessageScoreCooldownSeconds < 0)
                throw new InvalidOperationException("MessageScoreCooldownSeconds must be non-negative.");

            if (string.IsNullOrWhiteSpace(RankingRolePrefix))
                throw new InvalidOperationException("RankingRolePrefix must not be empty.");
        }


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


        // role‐assignment utilities --------------------------------------------

        /// <summary>
        /// Builds a Discord role name for a level,
        /// applying the configured <see cref="RankingRolePrefix"/>.
        /// </summary>
        public string BuildRankingRoleName(string levelName)
        {
            if (string.IsNullOrWhiteSpace(levelName))
                throw new ArgumentException("levelName cannot be empty.", nameof(levelName));

            var safe = levelName.Trim().Replace(' ', '_');
            return $"{RankingRolePrefix}{safe}";
        }


        // XP calculation -------------------------------------------------------

        /// <summary>
        /// Computes total XP to award for one message:
        /// base + optional booster + random bonus.
        /// </summary>
        /// <param name="boosterActive">Whether a booster is currently active.</param>
        /// <param name="randomProvider">
        /// Optional custom RNG (0..RandomXpBonus). Defaults to <see cref="Random"/>.
        /// </param>
        public int CalculateXpAward(
            bool boosterActive,
            Func<int> randomProvider = null)
        {
            var total = BaseXpPerMessage;

            if (boosterActive)
                total += BoosterXp;

            if (RandomXpBonus > 0)
            {
                randomProvider ??= () => new Random().Next(0, RandomXpBonus + 1);
                total += randomProvider();
            }

            return total;
        }
    }
}
