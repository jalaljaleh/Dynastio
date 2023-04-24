using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Dynastio.Bot
{
    internal class Program
    {
        public static void Main(string[] arg) => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task MainAsync()
        {
            Global.Main.Log("Main Async", "Started");

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            };

            var services = new ServiceCollection()
               .AddSingleton<DiscordSocketClient>()
               .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
               .AddSingleton<CommandService>()
               .AddSingleton<EventHandler>()
               .BuildServiceProvider();

        }
    }
}