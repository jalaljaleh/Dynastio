using Dynastio.Net;

namespace Dynastio.Bot.Test
{
    public class Program
    {
        public static void Main()
        {
            int ii = 0;
            int iii = 0;
           for(int i =0;i < 42;i++)
            {
                ii += getMax(i);
                iii += (int)CalculateReward(i);
                Console.WriteLine(i + "    " + getMax(i) + "    " + ii + "        " + CalculateReward(i) + "     " + iii);
            }
            Console.WriteLine(ii);
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
            return (((lvl + 250) * (int)Math.Pow(lvl + 1, 2.1))) + 7200;
        }
    }

}