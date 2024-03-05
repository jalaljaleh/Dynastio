using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Extenstions
{
    public static class IntegerExtensions
    {
        public static string ToRegularCounter(this int number)
        {
            string result = $"{number}";
            if (number < 10)
            {
                result = "0" + number;
            }

            return result;
        }

        public static string Metric(this ulong score)
        {
            if (score < 1) score = 1;
            if (score < 1000) return score.ToString();

            string score_ = "0.0x";

            if (score >= 100000000000000000)
            {
                score_ = string.Format("{0}E", Math.Round(score / 100000000000000000.0, 1));
                goto Return;
            }
            if (score >= 100000000000000)
            {
                score_ = string.Format("{0}P", Math.Round(score / 100000000000000.0, 1));
                goto Return;
            }
            if (score >= 100000000000)
            {
                score_ = string.Format("{0}T", Math.Round(score / 100000000000.0, 1));
                goto Return;
            }
            if (score >= 1000000000)
            {
                score_ = string.Format("{0}G", Math.Round(score / 1000000000.0, 1));
                goto Return;
            }
            if (score >= 1000000)
            {
                score_ = string.Format("{0}M", Math.Round(score / 1000000.0, 1));
                goto Return;
            }
            if (score >= 1000)
            {
                score_ = string.Format("{0}K", Math.Round(score / 1000.0, 1));
            }
        Return:
            return score_.ToString().Replace("/", ".");

        }
        public static string Metric(this long score)
        {
            return ulong.Parse(score.ToString()).Metric();
        }
        public static string Metric(this int score)
        {
            return ulong.Parse(score.ToString()).Metric();
        }
    }
}
