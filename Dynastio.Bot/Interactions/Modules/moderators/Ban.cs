using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.modules.moderators
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [DefaultMemberPermissions(GuildPermission.KickMembers)]
    public class banModule : CustomInteractionModuleBase
    {
        public const ulong _adminRoleId = 1105914502614089739;

        [SlashCommand("ban", "ban user")]
        public async Task ban(IGuildUser user, int MessagePruneDays = 0, string reason = "not provided")
        {
            await DeferAsync();

            if (!user.RoleIds.Contains(_adminRoleId))
                await FollowupAsync("Access Denied");            

            await user.BanAsync(MessagePruneDays, reason);

            await FollowupAsync(
                text: userMention, 
                embed: 
                $"{user.Mention} has been banned by {userMention} for `{reason}` reason."
                .ToEmbed($"{user.Mention} Banned !",user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));

        }

        [UserCommand("ban")]
        public async Task ban_(IGuildUser user)
        {
            await DeferAsync();

            if (!user.RoleIds.Contains(_adminRoleId))
                await FollowupAsync("Access Denied");

            await user.BanAsync(7, "");

            await FollowupAsync(
                text: userMention,
                embed:
                $"{user.Mention} has been banned by {userMention}."
                .ToEmbed($"{user.Mention} Banned !", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
        }

    }

}
