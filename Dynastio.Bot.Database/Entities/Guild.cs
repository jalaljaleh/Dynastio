using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
        [BsonId]
        public ulong Id { get; set; }
        public GuildSubscription Subscription { get; set; } = new();
        public RankingSettings RankingSettings { get; set; } = new();

        //public bool IsDeleteMessageEnabled { get; set; }
        public ulong PartnersRoleId { get; set; }

        public bool HasSubscription()
        {
            return Subscription.EndsAt > DateTime.UtcNow;
        }


     
    }

    public interface GuildModuleBase
    {
        bool IsEnabled { get; set; }
    }

    //public class GuildDeleteMessageModule : GuildModuleBase
    //{
    //    public GuildDeleteMessageModule()
    //    {
    //        IsEnabled = false;
    //    }
    //    public bool IsEnabled { get; set; }

    //    public ulong DefaultLogChannelId { get; set; }
    //    public ulong ModeratorsLogChannelId { get; set; }

    //}

    [BsonIgnoreExtraElements]
    public class RankingSettings : GuildModuleBase
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
    [BsonIgnoreExtraElements]
    public class GuildSubscription
    {
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public ulong UserId { get; set; }
        public List<string> History { get; set; }
    }
}


