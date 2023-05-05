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
            BoostServer,
            CoinRandom,
            Coin100 = 50,
            Coin200,
            Coin300,
            Coin400,
            Coin500,
            Coin1000,
            Coin1500,
        }
    }

}
