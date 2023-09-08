namespace Dynastio.Bot.Global
{
    public class Main
    {
        public const string version = "0.1.1.5-beta";

        public static DateTime StartUp { get; private set; } = DateTime.UtcNow;

        public static Random Random = new Random();

        public static bool IsDebug()
        {
#if DEBUG
            return true;
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