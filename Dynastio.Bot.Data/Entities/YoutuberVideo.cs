using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Dynastio.Bot.Data
{
    [BsonIgnoreExtraElements]
    public class YoutuberVideo
    {
        public string videoId { get; set; }
        public ulong user { get; set; }
        public DateTime createdAt { get; set; }
        public string GetUrl()
        {
            return "https://www.youtube.com/watch?v=" + videoId;
        }
    }
}
