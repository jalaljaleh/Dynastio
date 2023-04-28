using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Leaderboardcoin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("coins")]
        public ulong Coin { get; set; }

        [JsonProperty("last_active_at")]
        public DateTime LastActiveAt { get; set; }

        [JsonProperty("latest_server")]
        public string LatestServer { get; set; }

    }
}
