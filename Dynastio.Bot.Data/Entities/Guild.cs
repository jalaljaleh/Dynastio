using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{

    [BsonIgnoreExtraElements]
    public class Guild
    {
        public Guild() { }
        public ulong Id { get; set; }
      

    }


}


