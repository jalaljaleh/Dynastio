using Dynastio.Net;

namespace Dynastio.Bot.Test
{
    public class Program
    {
        public static void Main()
        {
            int ii = 0;
            int iii = 0;
            for (int i = 0; i < 42; i++)
            {
                ii += getMax(i);
                iii += (int)CalculateReward(i);
                Console.WriteLine(i + "    " + getMax(i) + "    " + ii + "        " + CalculateReward(i) + "     " + iii + "     " + CaclculateMessage(getMax(i)) + " msg");
            }
            Console.WriteLine(ii);
        }
        static double CaclculateMessage(int xp)
        {
            return xp / 50;
        }
        static double CalculateReward(int level)
        {
            const int maxLevel = 40;
            const double maxReward = 10000;
            double b = 1.0 / maxLevel;

            double a = maxReward / (Math.Exp(1) - 1);

            return Math.Round(a * (Math.Exp(b * level) - 1));
        }
        public async Task Test()
        {
            DynastioClient dc = new DynastioClient("");


            Console.ReadLine();
        }
        public static int getMax(int lvl)
        {
            if (lvl is 0)
                return _getMax(lvl);

            return _getMax(lvl) + 10000;

            int _getMax(int _lvl) => (_lvl < 21 ? 900 : + 600) * (int)Math.Pow(_lvl + 1, 2.1);
        }
    }

}