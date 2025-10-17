
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Moderator
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(Discord.GuildPermission.ModerateMembers)]
    public class WarnModule : MenuModulesBase
    {
        //[SlashCommand("warn", "warn a user with official discod bot")]
        //public async Task Warn(IUser user, string warnMessage, [Choice("Channel", "1"), Choice("DM", "2")] int warnPlace)
        //{
        //    await DeferAsync(true);

        //    var targetChannel = warnPlace == 1 ? Context.Channel : (IChannel)user.CreateDMChannelAsync();


        //    ComponentBuilderV2 cb = new ComponentBuilderV2();
        //    cb.WithTextDisplay(user.Mention);
        //    cb.

        //    await(targetChannel as ITextChannel).SendMessageAsync(components: cb.Build());
        //}
    }
}
