using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services.XpRankingSystem
{
    public static class XpCalculator
    {
        public const int MaxLevel = 40;

        public static int GetCurrentLevelXpRequirement(int level) =>
            (level + 1) * 510 * (int)Math.Pow(level + 1, 1.2);

        public static int GetLevelUpRequirementXp(int level, int currentXp) =>
            GetCurrentLevelXpRequirement(level) - currentXp;

        public static double GetLevelCoinsReward(int level)
        {
            double growthRate = 0.025;
            double baseReward = 10000.0 / (Math.Exp(1.0) - 1.0);
            return Math.Round(baseReward * (Math.Exp(growthRate * level) - 1.0));
        }
    }
}
