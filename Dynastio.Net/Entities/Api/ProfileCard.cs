using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class ProfileCard
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }
      
        [JsonProperty("chest")]
        public Personalchest Chest { get; set; }
       
        [JsonProperty("stat")]
        public PlayerStat Stat { get; set; }
    }
    internal class ProfileCardEntitiy
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }

        [JsonProperty("chest")]
        public string Chest { get; set; }

        [JsonProperty("stat")]
        public string Stat { get; set; }
    }
}
