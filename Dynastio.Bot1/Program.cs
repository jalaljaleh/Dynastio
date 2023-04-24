using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions;
using Dynastio.Data;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class Program
    {
        public const int ImageOnlyChannelsSlowMode = 30;
        public static Random Random = new Random();
        public static DateTime StartUp { get; } = DateTime.UtcNow;
        public const string FilePathConfigurationMain = @"C:\Users\Jalal Jaleh\OneDrive\Members\Jaleh Jalal\projects\Dynastio\dynastio.json";
        public const string FilePathConfigurationDebug = @"C:\Users\Jalal Jaleh\OneDrive\Members\Jaleh Jalal\projects\Dynastio\dynastio.debug.json";
        public static bool IsYoutubeServiceInitialized = false;
        public static void Main(string[] arg) => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task MainAsync()
        {
            Program.Log("Main Async", "Started");

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            };

            var configuration = Configuration.Get(false ? FilePathConfigurationMain : FilePathConfigurationDebug);

            //LocaleService.StartTranslateProccess();
            var languages = new LocaleService();

            var graphicService = new GraphicService().Initialize();

            IDatabaseContext db = configuration.DatabaseConnectionString.IsNullOrEmpty()
                ? new DirectoryDbContext(configuration.DatabaseConnectionString)
                : configuration.DatabaseConnectionString.Contains("mongodb")
                        ? new MongoDbContext(configuration.DatabaseConnectionString)
                        : new DirectoryDbContext(configuration.DatabaseConnectionString);

            db = await db.InitializeAsync();

            var dynastClient = new DynastioClient(configuration.DynastioApi);

            var userService = new UserService(db, dynastClient);

            var guildService = new GuildService(db);

            var youtubeService = new YoutubeService(configuration.YoutubeApiKey, configuration.DynastioYoutubeChannelId);
            await youtubeService.InitializeAsync();
                      
            var services = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton(dynastClient)
                .AddSingleton((IDatabaseContext)db)
                .AddSingleton(userService)
                .AddSingleton(guildService)
                .AddSingleton(graphicService)
                .AddSingleton(youtubeService)
                .AddSingleton(_socketConfig)
                .AddSingleton(languages)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<EventHandler>()
                .BuildServiceProvider();

            await RunAsync(services);
        }
        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
            AlwaysDownloadUsers = true,
            AlwaysDownloadDefaultStickers = false,
            DefaultRetryMode = RetryMode.AlwaysRetry,

        };
        public async Task RunAsync(IServiceProvider _services)
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            _services.GetRequiredService<EventHandler>().Initialize();
            // Here we can initialize the service that will register and execute our commands
            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
            await _services.GetRequiredService<CommandHandler>().InitializeAsync();

            // Bot token can be provided from the Configuration object we set up earlier
            await client.LoginAsync(TokenType.Bot, _services.GetRequiredService<Configuration>().BotToken);
            await client.StartAsync();

            // Never quit the program until manually forced to.
            await Task.Delay(Timeout.Infinite);

        }
        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
        public static void Log(string service, string text)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("T") + " " + service.PadRight(20) + text);
        }
        public static void Log(string service, string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(DateTime.UtcNow.ToString("T") + " " + service.PadRight(20) + text);
            Console.ResetColor();
        }
        public static bool IsDebug()
        {
#if DEBUG
                return true;
#else
                return false;
#endif
        }
    }
}
