using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Globalization;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Dynastio.Bot.Entities;
using Dynastio.Bot.Database;
using Dynastio.Graphic;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Services;

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

            try
            {
                new Program()
                    .MainAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //Task.Delay(TimeSpan.FromMinutes(20));
        }

        public async Task MainAsync()
        {
            Global.GlobalMain.Log("Main Async", "Started");

            // Environment.SetEnvironmentVariable("config-key", "");
            AppConfiguration configuration = AppConfiguration.LoadConfiguration();
            var services = new ServiceCollection();
            services
           .AddSingleton(configuration)

           .AddSingleton<DynastioApi>(x => new DynastioApi(configuration.Tokens["dynastio-api"]))
           .AddSingleton<DynastioBotDatabase>()
           .AddSingleton<DynastioGraphic>()
           .AddSingleton<DynastioBotGlobalization>()

           .AddSingleton<UserService>()

           .AddSingleton<XpRankingSystemService>()
           .AddSingleton<BadgesBridgeService>()

           .AddSingleton<InteractionsHandler>()
           .AddSingleton<InteractionService>()
           .AddSingleton<EventsHandler>()
           .AddSingleton<MessagesHandler>()

           .AddSingleton<RepeaterService>()
           .AddSingleton<AdvertisingService>()

           .AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new()
           {
               GatewayIntents = GatewayIntents.All,
               AlwaysDownloadUsers = true,

               MessageCacheSize = 1024,
               AlwaysDownloadDefaultStickers = false,
               DefaultRetryMode = RetryMode.AlwaysRetry,

               UseSystemClock = false,
               UseInteractionSnowflakeDate = false,
           }));


            await RunAsync(services.BuildServiceProvider());
        }
        public async Task RunAsync(IServiceProvider _services)
        {
            var configuration = _services.GetService<AppConfiguration>();

            _services.GetRequiredService<DynastioApi>();

            await _services.GetService<DynastioBotDatabase>()
                 .InitializeAsync(configuration.Tokens["connectionstring-mongodb"], DynastioBotDatabase.DatabasesInstances.Mongodb, GlobalMain.IsDebug());

            _services.GetRequiredService<DynastioGraphic>()
                .Initialize();

            _services.GetRequiredService<DynastioBotGlobalization>()
                .Initialize();

            _services.GetRequiredService<UserService>();

            _services.GetRequiredService<XpRankingSystemService>();
            _services.GetRequiredService<BadgesBridgeService>();

            _services.GetRequiredService<EventsHandler>();
            _services.GetRequiredService<MessagesHandler>();

            await _services.GetRequiredService<InteractionsHandler>().InitializeAsync();

            //  await _services.GetRequiredService<AdvertisingService>().InitializeAsync();

            var client = _services.GetRequiredService<DiscordSocketClient>();
            client.Log += (LogMessage arg) =>
            {
                Console.WriteLine(arg.ToString());
                return Task.CompletedTask;
            };

            await client.LoginAsync(TokenType.Bot, configuration.Tokens["discord-bot"]);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

    }
}
