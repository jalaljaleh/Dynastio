using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands.Chat
{
    [RequireContext(ContextType.Guild)]
    public class ChatModule : ModuleBase<BotSocketCommandContext>
    {
        [Command(text: "hi", true, aliases: ["hello", "ky"])]
        public async Task HiAsync()
        {
            await Context.Message.ReplyAsync("Hello, how are you today ?");
        }
    }
}
