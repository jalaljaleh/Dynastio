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
    public class Help : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public InteractionService InteractionService { get; set; }
        [SlashCommand("help", "help")]
        public async Task help()
        {
            await DeferAsync();
            await FollowupAsync(embed: "This command is not available yet.".ToEmbed("Command Is Disabled", Context.Client.CurrentUser.GetAvatarUrl()));
        }
    }
}
