using Dynastio.Net;

namespace Dynastio.Bot.Test
{
    public class Program
    {
        public static void Main()
        {
            int ii = 0;
           for(int i =0;i < 42;i++)
            {
                Console.WriteLine(i + "    " + getMax(i));
                ii += getMax(i);
            }
            Console.WriteLine(ii);
        }
        public async Task Test()
        {
            DynastioClient dc = new DynastioClient("");


            Console.ReadLine();
        }
        public static int getMax(int lvl)
        {
                return (((lvl + 147) * (int)Math.Pow(lvl, 1.1))) ;
        }
    }

}