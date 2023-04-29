using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<CustomSocketCommandContext>
    {

        [Command("ping")]
        [Alias("pong", "hello")]
        public async Task PingAsync()
        {
            await ReplyAsync("pong!");

        }


    }
}
