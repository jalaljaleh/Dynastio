namespace Dynastio.Bot.Global
{
    public class Main
    {
        public Main()
        {
            version = "0.4.1-beta" + Random.Next(0, 1000);
        }
        public static string version { get; internal set; } = "";

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