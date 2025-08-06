
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
        [BsonId]
        public ulong Id { get; set; }

        public XpSystemSettings XpSystemSettings { get; set; } = new();
        public BadgeBridgeSettings BadgeBridgeSettings { get; set; } = new();




    }

}


