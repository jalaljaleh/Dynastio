using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class UserSurroundingRankRow
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

    }
    public class UserSurroundingRank
    {
        public UserSurroundingRank(string id)
        {
            OwnerId = id;
        }
        public string OwnerId { get; set; }
        public UserSurroundingRankRow Daily { get => UsersRankDaily.Where(a => a.Id == OwnerId).FirstOrDefault(); }
        public UserSurroundingRankRow Weekly { get => UsersRankWeekly.Where(a => a.Id == OwnerId).FirstOrDefault(); }
        public UserSurroundingRankRow Montly { get => UsersRankMontly.Where(a => a.Id == OwnerId).FirstOrDefault(); }

        public List<UserSurroundingRankRow> UsersRankDaily { get; set; }
        public List<UserSurroundingRankRow> UsersRankWeekly { get; set; }
        public List<UserSurroundingRankRow> UsersRankMontly { get; set; }
    }

}
