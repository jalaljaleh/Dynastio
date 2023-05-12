using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Managers;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;

namespace Dynastio.Bot
{
    internal class Program
    {
        public static void Main(string[] arg)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            };

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Global.Main.Log("Main Async", "Started");

            var configuration = Configuration.LoadConfiguration();

            var _db = new DynastioBotDatabase();
            var db = await _db.GetInstanseAsync(configuration.MongodbConnection, DynastioBotDatabase.DatabasesInstances.Mongodb);

            var services = new ServiceCollection()
               .AddSingleton(configuration)
               .AddSingleton<DynastioBotDatabase>(_db)
               .AddSingleton<IDynastioBotDatabase>(db)
               .AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new()
               {
                   GatewayIntents = GatewayIntents.All,
                   AlwaysDownloadUsers = true,
                   AlwaysDownloadDefaultStickers = false,
                   DefaultRetryMode = RetryMode.AlwaysRetry,
               }))

               .AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
               .AddSingleton<Handlers.EventHandler>()
               .AddSingleton<CommandService>()
               .AddSingleton<InteractionsHandler>()
               .AddSingleton<CommandsHandler>()
               .AddSingleton<MessageHandler>()


               .AddSingleton<DynastioClient>(x => new DynastioClient(configuration.DynastioApi))
               
               .AddSingleton<UserService>()
               .AddSingleton<GuildService>()

               .AddSingleton<RankService>()
               .AddSingleton<GraphicService>()

               .AddSingleton<GlobalizationService>()
               .AddSingleton<YoutubeService>()
               .AddSingleton<InternetService>()
               .BuildServiceProvider();


            await RunAsync(services);

        }
        public async Task RunAsync(IServiceProvider _services)
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += (LogMessage arg) =>
            {
                Console.WriteLine(arg.ToString());
                return Task.CompletedTask;
            };

            _services.GetRequiredService<GlobalizationService>().LoadDirectory(FileManager.ToResourcePath("globalization"));
            _services.GetRequiredService<YoutubeService>();
            _services.GetRequiredService<GraphicService>().Initialize();

            _services.GetRequiredService<Handlers.EventHandler>();
            _services.GetRequiredService<MessageHandler>();

            await _services.GetRequiredService<InteractionsHandler>().InitializeAsync();
            await _services.GetRequiredService<CommandsHandler>().InitializeAsync();


            await client.LoginAsync(TokenType.Bot, _services.GetRequiredService<Configuration>().BotToken);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }


    }
}
