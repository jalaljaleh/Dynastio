using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Events;
using Dynastio.Bot.Handlers;
using Newtonsoft.Json;

namespace Dynastio.Bot.Logs
{
    public class LoggerBot
    {
        public async Task RunAsync(string token)
        {
            var client = new DiscordSocketClient(new()
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = false,

                MessageCacheSize = 1024,
                AlwaysDownloadDefaultStickers = false,
                DefaultRetryMode = RetryMode.AlwaysRetry,

                UseSystemClock = false,
                UseInteractionSnowflakeDate = false,
            });

            client.Log += (LogMessage arg) =>
            {
                Console.WriteLine(arg.ToString());
                return Task.CompletedTask;
            };

            MessagesHandler msgHandler = new MessagesHandler(client, 1120683501256188005);
            EventsHandler eventHandler = new EventsHandler(client);

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

    }
}
