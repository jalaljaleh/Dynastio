
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Player
    {
        public Player()
        {
        }
        internal Player Update(Server parent)
        {
            Parent = parent;
            UniqeId = "hash$" + (Parent.Label + InternalId).GetHashCode();
            return this;
        }

        [JsonIgnore]
        public string UniqeId { get; set; } = "";
        [JsonIgnore]
        public bool IsAuth => !string.IsNullOrEmpty(Id);
        [JsonIgnore]
        public Server Parent { get; set; }


        [JsonProperty("internal_id")]
        public ulong InternalId { get; set; } = 0;

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("name")]
        public string Nickname { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }



    }
}
