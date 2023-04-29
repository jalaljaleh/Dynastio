using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class FeaturedVideos
    {
        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("texture")]
        public string Texture { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("expireAt")]
        public DateTime ExpireAt { get; set; }

        [JsonProperty("__v")]
        public int V { get; set; }
    }
}
