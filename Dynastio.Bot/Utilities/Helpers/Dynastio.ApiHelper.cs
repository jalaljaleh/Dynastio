using Discord.Rest;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class DynastioHelper
    {
        private static string _tabledlevelrewards;
        public static string TabledLevelRewards
        {
            get
            {
                if (string.IsNullOrEmpty(_tabledlevelrewards))
                    _tabledlevelrewards = GetTabledLevelReward();

                return _tabledlevelrewards;
            }
        }
        private static double _tabledlevelrewardstatal = 0;
        public static double TotalRankingReward
        {
            get
            {
                if (_tabledlevelrewardstatal == 0)
                    _tabledlevelrewardstatal = CalcualteTotalReward();

                return _tabledlevelrewardstatal;
            }
        }
        public static double CalcualteTotalReward()
        {
           return Enumerable.Range(1, 40).Select(a=> DynastioApiHelper.GetLevelCoinsReward(a)).ToList().Sum();
        }
        public static string GetTabledLevelReward()
        {
            var levels = Enumerable.Range(1, 40)
             .Select(a => new { Level = a, Reward = DynastioApiHelper.GetLevelCoinsReward(a) })
             .ToList();

            var tripletLevels = Enumerable.Range(0, (levels.Count + 2) / 3)
                .Select(i => new
                {
                    Level1 = levels[i * 3].Level,
                    Reward1 = levels[i * 3].Reward,
                    Level2 = (i * 3 + 1 < levels.Count) ? levels[i * 3 + 1].Level : 0,
                    Reward2 = (i * 3 + 1 < levels.Count) ? levels[i * 3 + 1].Reward : 0,
                    Level3 = (i * 3 + 2 < levels.Count) ? levels[i * 3 + 2].Level : 0,
                    Reward3 = (i * 3 + 2 < levels.Count) ? levels[i * 3 + 2].Reward : 0
                });

            var table = tripletLevels
                .ToFormattedTable(
                    new[] { "Level", "Coins", "Level", "Coins ", "Level", "Coins" },
                    new Func<dynamic, object>[] { a => a.Level1, a => a.Reward1, a => a.Level2, a => a.Reward2, a => a.Level3, a => a.Reward3 }
                )
                .ToCodeBlock();

            return table;
        }

    }
}
