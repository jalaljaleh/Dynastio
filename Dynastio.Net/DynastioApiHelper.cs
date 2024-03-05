using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class DynastioApiHelper
    {
        public const int MaxLevel = 40;
        public static double GetLevelCoinsReward(int level)
        {
            double b = 1.0 / MaxLevel;

            double a = 10000 / (Math.Exp(1) - 1);

            return Math.Round(a * (Math.Exp(b * level) - 1));
        }
    }
}
