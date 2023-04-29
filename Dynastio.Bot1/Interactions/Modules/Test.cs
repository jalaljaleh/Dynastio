using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules
{

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]

    public class Test : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [SlashCommand("test", "test something")]
        [AvoidUserDuplicateRun()]
        public async Task test()
        {
            await DeferAsync();
            await FollowupAsync("wait 20s " + DateTime.UtcNow.AddSeconds(20).ToDiscordUnixTimestampFormat());
            await Task.Delay(20000);
        }
    }
}
