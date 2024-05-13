using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class GuildSubscription
    {
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public ulong UserId { get; set; }
        public List<string> History { get; set; }
    }
}
