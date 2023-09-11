using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class DiscordRank
    {
        public DiscordRank(int rank)
        {
            Rank = rank;
        }

        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
}
