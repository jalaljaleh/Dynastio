using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Dynastio.Net
{
    public enum LeaderboardScoreItemId
    {
        Day,
        Week,
        Month,
        Solo_Day,
        Solo_Week,
        Solo_Month,
        PVP_Day,
        PVP_Week,
        PVP_Month,
    }
    public class Leaderboardscore
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public LeaderboardscoreItem[] Data { get; set; }


        public static Leaderboardscore GetLeaderboard(List<Leaderboardscore> _value, LeaderboardScoreItemId type)
        {
            return type switch
            {
                LeaderboardScoreItemId.Day => _value.FirstOrDefault(a => a.Id == "day_sum"),
                LeaderboardScoreItemId.Week => _value.FirstOrDefault(a => a.Id == "week_sum"),
                LeaderboardScoreItemId.Month => _value.FirstOrDefault(a => a.Id == "month_sum"),

                LeaderboardScoreItemId.PVP_Day => _value.FirstOrDefault(a => a.Id == "pvp.day_sum"),
                LeaderboardScoreItemId.PVP_Week => _value.FirstOrDefault(a => a.Id == "pvp.week_sum"),
                LeaderboardScoreItemId.PVP_Month => _value.FirstOrDefault(a => a.Id == "pvp.month_sum"),

                LeaderboardScoreItemId.Solo_Day => _value.FirstOrDefault(a => a.Id == "solo.day_sum"),
                LeaderboardScoreItemId.Solo_Week => _value.FirstOrDefault(a => a.Id == "solo.week_sum"),
                LeaderboardScoreItemId.Solo_Month => _value.FirstOrDefault(a => a.Id == "solo.month_sum"),


                _ => _value.FirstOrDefault(a => a.Id == "month_sum")
            };
        }

    }

    public class LeaderboardscoreItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

}
