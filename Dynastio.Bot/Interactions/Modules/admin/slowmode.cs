using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Bot.Interactions.modules.moderators;

namespace Dynastio.Bot.Interactions.modules.admin
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.ManageChannels)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    public class slowmodeModule : CustomInteractionModuleBase
    {

        [SlashCommand("channel-slowmode-set", "Set Slowmode")]
        public async Task slowmode(ITextChannel channel, TimeType time, int value)
        {
            await DeferAsync();
            var slowModeInterval = value * (int)time;
            if (slowModeInterval <= 21600) // api limit
            {
                await channel.ModifyAsync(a =>
                {
                    a.SlowModeInterval = slowModeInterval;
                });
                await FollowupAsync(embed: $"Slowmode is ready for this channel <#{channel.Id}>".ToEmbed());
                return;
            }
            await FollowupAsync(embed: "Can not set more than 6 hours.".ToEmbed());
        }

    }

}
