using System.Net.NetworkInformation;
using System.Reflection;

namespace Dynastio.Bot
{
    public class Common
    {

        public static DateTime StartUp { get; private set; } = DateTime.UtcNow;

        public static Random Random = new Random();

        public static bool Developer { get; set; } = true;
        public static bool IsDebug()
        {
#if DEBUG
            return true && !Developer;
#else
            return false;
#endif
        }
        public static void Log(string service, string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Log(service, text);
            Console.ResetColor();
        }
        public static void Log(string service, string text)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("T") + " " + service.PadRight(20) + text);
        }
    }
}