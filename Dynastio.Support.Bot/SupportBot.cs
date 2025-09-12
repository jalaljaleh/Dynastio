using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dynastio.Support.Bot
{
    public class SupportBot
    {
        public SupportBot()
        {
          
        }
        public Task InitializeAsync(string botToken)
        {
            try
            {


                // Kick off the Discord client loop without awaiting it
                _ = Task.Run(() => StartDiscordClientAsync(BuildServiceProvider(), botToken)
                                  .ContinueWith(t =>
                                  {
                                      // log exceptions if StartDiscordClientAsync ever faults
                                      Console.WriteLine(t.Exception);
                                  },
                                  TaskContinuationOptions.OnlyOnFaulted));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error during main:");
                Console.WriteLine(ex);

            }

            return Task.CompletedTask;
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

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


            return services.BuildServiceProvider();
        }

        private static async Task StartDiscordClientAsync(ServiceProvider sp, string token)
        {
            var client = sp.GetRequiredService<DiscordSocketClient>();

            client.Log += log =>
            {
                Console.WriteLine(log);
                return Task.CompletedTask;
            };

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Prevent the application from exiting
            await Task.Delay(Timeout.Infinite);
        }

    }
}