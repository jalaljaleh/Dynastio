using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{

    [BsonIgnoreExtraElements]
    public class Guild
    {
        public Guild()
        {
            RankingSettings = new RankingSettings()
            {
                IsEnabled = false,
                MessageDelay = 60,
                XpBoosters = 10,
                XpPerMessage = 40,
                XpRandom = 10,
            };
            Subscription = new GuildSubscription()
            {
                History = new()
            };
        }
        public ulong Id { get; set; }
        public GuildSubscription Subscription { get; set; }
        public RankingSettings RankingSettings { get; set; }
        public bool HasSubscription()
        {
            return Subscription.EndsAt > DateTime.UtcNow;
        }
    }

    public class RankingSettings
    {
        public bool IsEnabled { get; set; }
        public bool IsGameRewardEnabled { get; set; }

        public int XpPerMessage { get; set; }
        public int XpBoosters { get; set; }
        public int XpRandom { get; set; }
        public int MessageDelay { get; set; }
        public string RolesPrefix { get; set; }
        public ulong LogChannelId { get; set; }
        public string WebhookUrl { get; set; }
        public List<ulong> ChannelIds { get; set; }
        public bool IsLevelUpChannel(ulong channelId)
        {
            return ChannelIds.Any(a => a == channelId);
        }
    }
    [BsonIgnoreExtraElements]
    public class GuildSubscription
    {
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public ulong UserId { get; set; }
        public List<string> History { get; set; }
    }
}


