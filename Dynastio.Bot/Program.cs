using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.EventHandlers;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class Program
    {
        //public static async Task MainTest()
        //{
        //    var config = ConfigurationService.Load();
        //    var dynast = new DynastioApi(config.Tokens["dynastio-api"]);

        //    var v = dynast.Leaderboardcoins;


        //    Environment.Exit(0);
        //}
        public static async Task Main(string[] args)
        {
            //  await MainTest();

            ConfigureJsonSerializer();

            try
            {
                var config = ConfigurationService.Load();
                using var serviceProvider = BuildServiceProvider(config);

                await InitializeInfrastructureAsync(serviceProvider, config);
                await StartDiscordClientAsync(serviceProvider, config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error during startup:");
                Console.WriteLine(ex);
            }
        }

        private static void ConfigureJsonSerializer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }
        private static readonly InteractionServiceConfig _interactionServiceConfig = new()
        {
            LocalizationManager = new ResxLocalizationManager("Dynastio.Bot.Services.GlobalizationService.InteractionCommands.InetactionCommandsLocale", Assembly.GetEntryAssembly(),
           new CultureInfo("en-US"), new CultureInfo("ru"))
        };

        private static ServiceProvider BuildServiceProvider(ConfigurationService config)
        {
            var services = new ServiceCollection();

            // Core configuration & APIs
            services.AddSingleton(config);
            services.AddSingleton(_ => new DynastioApi(config.Tokens["dynastio-api"]) { });
            services.AddSingleton<DynastioBotDatabase>();
            services.AddSingleton<DynastioGraphic>();
            services.AddSingleton<AssetUrlService>();

            // services
            services.AddSingleton<UsersService>();
            services.AddSingleton<EmoteService>();
            services.AddSingleton<XpRankingSystemService>();
            services.AddSingleton<BadgesRoleSyncService>();
            services.AddSingleton<RepeaterService>();

            // Discord
            services.AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1024,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                UseSystemClock = false,
                UseInteractionSnowflakeDate = false
            }));

            services.AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig));
            services.AddSingleton<InteractionsHandler>();
            services.AddSingleton<MessagesHandler>();


            // Event handlers
            services.AddSingleton<IDiscordEvent, UserJoinedEvent>();
            services.AddSingleton<IDiscordEvent, GuildJoinedEvent>();
            services.AddSingleton<IDiscordEvent, BotReadyEvent>();
            services.AddSingleton<EventsHandler>();

            return services.BuildServiceProvider();
        }

        private static async Task InitializeInfrastructureAsync(ServiceProvider sp, ConfigurationService config)
        {
            // Database
            var db = sp.GetRequiredService<DynastioBotDatabase>();

            if (Common.IsDebug())
                await db.InitializeAsync(null, DynastioBotDatabase.DatabasesInstances.FileDb, default);
            else
                await db.InitializeAsync(config.Tokens["connectionstring-mongodb"], DynastioBotDatabase.DatabasesInstances.Mongodb, Common.IsDebug());


            // Graphics & Localization
            sp.GetRequiredService<DynastioGraphic>().Initialize();

            // Warm up services
            sp.GetRequiredService<UsersService>();
            sp.GetRequiredService<XpRankingSystemService>();
            sp.GetRequiredService<BadgesRoleSyncService>();
            sp.GetRequiredService<EmoteService>();

            // Handlers
            var eventsHandler = sp.GetRequiredService<EventsHandler>();
            await eventsHandler.InitializeAsync();

            sp.GetRequiredService<MessagesHandler>();
            await sp.GetRequiredService<InteractionsHandler>().InitializeAsync();

        }

        private static async Task StartDiscordClientAsync(ServiceProvider sp, ConfigurationService config)
        {
            var client = sp.GetRequiredService<DiscordSocketClient>();

            client.Log += log =>
            {
                Console.WriteLine(log);
                return Task.CompletedTask;
            };

            await client.LoginAsync(TokenType.Bot, Common.IsDebug() ? config.DebugDiscordBotToken : config.Tokens["discord-bot"]);
            await client.StartAsync();

            // Prevent the application from exiting
            await Task.Delay(Timeout.Infinite);
        }

    }
}