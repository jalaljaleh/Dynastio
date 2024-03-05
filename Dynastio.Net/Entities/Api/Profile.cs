using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Profile
    {
        public Profile()
        {

        }

        [JsonProperty("coins")]
        public int Coins { get; set; }

        [JsonProperty("badges")]
        public List<BadgeType> Badges { get; set; } = new();

        [JsonProperty("latest_server")]
        public string LatestServer { get; set; }

        [JsonProperty("last_active_at")]
        public DateTime LastActiveAt { get; set; } = new();

        [JsonProperty("unlocked_recipes")]
        public List<ItemType> UnlockedRecipes { get; set; } = new();

        [JsonProperty("unlocked_skins")]
        public List<SkinType> UnlockedSkins { get; set; } = new();

        [JsonProperty("unlocked_buildings")]
        public List<EntityType> UnlockedBuildings { get; set; } = new();

        [JsonProperty("experience")]
        public int Experience { get; set; }
        
        [JsonProperty("level")]
        public int Level { get; set; }

        public float GetExperienceMax()
        {
            return 250 + Level * 500;
        }
        public float GetExperience(int value)
        {
            return GetExperience() * value / GetExperienceMax();
        }
        public float GetRequireExperienceForNewLevel()
        {
            return GetExperienceMax() - GetExperience();
        }
        public float GetExperience()
        {
            return Experience;
        }
    }

}
