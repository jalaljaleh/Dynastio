using Dynastio.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

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
        public ModuleRankingRole RankingSettings { get; set; } = new();
        public ModuleDynastioBadgesRole BadgesRole { get; set; } = new();
        public ulong PartnersRoleId { get; set; }

        public bool HasSubscription()
        {
            return Subscription.EndsAt > DateTime.UtcNow;
        }


    }

}


