using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Modules.Interactions
{
    [EnabledInDm(false)]
    [RequireGuild(RequireGuild.LocalGuildId.Main)]
    [RequireChannel(RequireChannel.LocalChannelId.Honor)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireContext(ContextType.Guild)]
    public class HonorModule : InteractionModuleBase<CustomSocketInteractionContext>
    {
        public UserService userManager { get; set; }

        public enum HonorType
        {
            Default
        }
        [RequireHonorTime]
        [SlashCommand("honor", "Get Random Honor")]
        public async Task Honor_()
        {
            await DeferAsync();
            await FollowupAsync(embed: $"Use **.honor** instead.".ToEmbed());
        }
    }

}
