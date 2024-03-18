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
           
        }
        public ulong Id { get; set; }
        public GuildSubscription Subscription { get; set; } = new();
        public RankingSettings RankingSettings { get; set; } = new();
        public bool HasSubscription()
        {
            return Subscription.EndsAt > DateTime.UtcNow;
        }
        public override string ToString()
        {
            return
                $"Id: {Id}\n" +
                $" **Rank Module**: {(RankingSettings.IsEnabled ? "Enabled" : "Disabled")}\n" +
                $"Id: {Id}\n" +
                $"Id: {Id}\n" +
                $"Id: {Id}\n" +
                $"Id: {Id}\n" +
                $"Id: {Id}\n" +
                $"";
        }
    }
    [BsonIgnoreExtraElements]
    public class RankingSettings
    {
        public RankingSettings()
        {
            IsEnabled = false;
            ChannelIds = new();
            IsGameRewardEnabled = false;
            LogChannelId = 0;
            RolesPrefix = "rank: ";
            Delay = 60;
            XpBoosters = 10;
            XpPerMessage = 40;
            XpRandom = 10;
        }
        public bool IsEnabled { get; set; }
        public bool IsGameRewardEnabled { get; set; }

        public int XpPerMessage { get; set; }
        public int XpBoosters { get; set; }
        public int XpRandom { get; set; }
        public int Delay { get; set; }
        public string RolesPrefix { get; set; }
        public ulong LogChannelId { get; set; }
        public List<ulong> ChannelIds { get; set; } = new();


        public bool IsLevelUpChannel(ulong channelId)
        {
            return ChannelIds.Any(a => a == channelId);
        }

    }

    public class GuildSubscription
    {
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public ulong UserId { get; set; }
        public List<string> History { get; set; }
    }
}


