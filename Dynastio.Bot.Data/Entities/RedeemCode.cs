using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    public class RedeemCode
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public RedeemType Type { get; set; }
        public string Code { get; set; }
       
        public enum RedeemType
        {
            Default,
            Boost_Server,
            Coin_100 = 50,
            Coin_200,
            Coin_300,
            Coin_400,
            Coin_500,
            Coin_1000,
            Coin_1500,
        }
    }

}
