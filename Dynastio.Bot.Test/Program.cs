using Dynastio.Net;

namespace Dynastio.Bot.Test
{
    public class Program
    {
        public static void Main()
        {
            new Program().Test().GetAwaiter().GetResult();
        }
        public async Task Test()
        {
            DynastioClient dc = new DynastioClient("");


            Console.ReadLine();
        }
    }

}