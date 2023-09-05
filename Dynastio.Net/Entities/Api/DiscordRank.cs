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
        [JsonProperty("rank")]
        public int Rank { get; set; }

    }
}
