using Dynastio.Net;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class BadgeRole
    {
        public BadgeType Badge { get; set; }
        public ulong RoleId { get; set; }

        public bool IsApplied() => RoleId != 0;
    }
}
