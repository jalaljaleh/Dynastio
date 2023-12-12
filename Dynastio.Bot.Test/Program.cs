using Dynastio.Net;

namespace Dynastio.Bot.Test
{
    public class Program
    {
        public static void Main()
        {
            int totallLevel = 0;
            int totallReward = 0;
            Console.WriteLine("Level\tXp\tReward\tMessages\tHours\tdays\tOffest");

            for (int level = 0; level < 42; level++)
            {
                totallLevel += getMax(level);
                totallReward += (int)CalculateLevelReward(level);

                int messages = getMax(level) / 50;
                int hours = messages / 60;
                int days = hours / 24;
                Console.WriteLine(level + "\t" + getMax(level) + "\t" + CalculateLevelReward(level) + "\t" + messages  + "\t\t" + hours + "\t" + days + "\t" + days * 12);
            }

        }
        public const int _nextScoreTime = 60;
        public const int _updateUserTime = 240;
        public const int maxLevel = 40;
        public const double maxReward = 10000;
        public const int _score = 50;
        public const int _boostersExpandableXp = 15;
        public const int _randomXp = 10;
        private ulong[] _score_channels = {
            480966712318099487, //
            486591124836974592, //
            1098632452274135112,//
            1098918867255967814,//
            1098248723013841026,//
            1098608343947415575,//
            1098263349873082438,//
        };
        public static int getMax(int lvl)
        {
            if (lvl is 0)
                return _getMax(lvl + 1);

            if (lvl is 1)
                return _getMax(lvl + 3);

            return _getMax(lvl);

            int _getMax(int _lvl) => (_lvl * 510) * (int)Math.Pow(_lvl + 1, 1.2);
        }

        public static double CalculateLevelReward(int level)
        {
            double b = 1.0 / maxLevel;

            double a = maxReward / (Math.Exp(1) - 1);

            return Math.Round(a * (Math.Exp(b * level) - 1));
        }
    }

}