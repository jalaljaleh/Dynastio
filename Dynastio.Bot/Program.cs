using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Data;
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

            new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();
        }

        public async Task MainAsync()
        {
            Global.Main.Log("Main Async", "Started");

           var configuration = Configuration.LoadConfiguration(false);
          // var configuration = Configuration.LoadReleaseConfiguration();
           //Configuration.UpdateConfiguration(configuration);


            var services = new ServiceCollection()
               .AddSingleton(configuration)
               .AddSingleton<DynastioData>(x => new DynastioData())
               .AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new()
                {
                    GatewayIntents = GatewayIntents.All,
                    AlwaysDownloadUsers = true,

                    MessageCacheSize = 1024,
                    AlwaysDownloadDefaultStickers = false,
                    DefaultRetryMode = RetryMode.AlwaysRetry,
                }))

               .AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
               .AddSingleton<Handlers.EventHandler>()
               .AddSingleton<CommandService>()
               .AddSingleton<InteractionsHandler>()
               .AddSingleton<CommandsHandler>()
               .AddSingleton<MessageHandler>()
               .AddSingleton<GuildMemberUpdatesHandler>()

               .AddSingleton<RepeaterService>()

               .AddSingleton<DynastioClient>(x => new DynastioClient(configuration.DynastioApi))

               .AddSingleton<UserService>()
               .AddSingleton<GuildService>()
               .AddSingleton<WebhookService>()

               .AddSingleton<RankService>()
               .AddSingleton<GraphicService>()
               .AddSingleton<GlobalizationService>()
               .AddSingleton<YoutubeService>()
               .AddSingleton<InternetService>()
               .AddSingleton<FeaturedVideosService>()
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

            var configuration = _services.GetService<Configuration>();

            await _services.GetService<DynastioData>()
                .InitializeAsync(configuration.DatabaseConnectionString, DynastioData.DatabasesInstances.Mongodb);

            _services.GetRequiredService<GlobalizationService>()
                .LoadDirectory(FileManager.ToResourcePath("globalization"));

            _services.GetRequiredService<YoutubeService>();
            _services.GetRequiredService<UserService>();

            _services.GetRequiredService<FeaturedVideosService>();
            _services.GetRequiredService<RepeaterService>();
            _services.GetRequiredService<GraphicService>().Initialize();

            _services.GetRequiredService<Handlers.EventHandler>();
            _services.GetRequiredService<MessageHandler>();
            _services.GetRequiredService<GuildMemberUpdatesHandler>();

            _services.GetRequiredService<WebhookService>();

            await _services.GetRequiredService<InteractionsHandler>().InitializeAsync();
            await _services.GetRequiredService<CommandsHandler>().InitializeAsync();


            await client.LoginAsync(TokenType.Bot, _services.GetRequiredService<Configuration>().BotToken);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }


    }
}
