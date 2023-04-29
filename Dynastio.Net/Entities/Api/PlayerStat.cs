using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class PlayerStat
    {
        [JsonProperty("kill")]
        public Dictionary<EntityType, int> Kill { get; set; }
        [JsonProperty("build")]
        public Dictionary<EntityType, int> Build { get; set; }
        [JsonProperty("death")]
        public Dictionary<EntityType, int> Death { get; set; }

        [JsonProperty("gather")]
        public Dictionary<ItemType, int> Gather { get; set; }
        [JsonProperty("craft")]
        public Dictionary<ItemType, int> Craft { get; set; }
        [JsonProperty("shop")]
        public Dictionary<ItemType, int> Shop { get; set; }

        [JsonProperty("suicide")]
        public int Suicide { get; set; }
        [JsonProperty("days_survived")]
        public int SurvivedDays { get; set; }

    }
}
