using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Owner;
using static Dynastio.Bot.Data.Guild;

namespace Dynastio.Bot.Interactions.Modules.Admin
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RateLimit(60, 2, RateLimit.RateLimitType.User)]
    [Group("admin", "server admin commands.")]
    public class serverbooster : CustomInteractionModuleBase
    {
        [Group("setup", "setup.")]
        public class SetupModule : OwnerModule
        {
            //[SlashCommand("badge-roles", "create dynastio badge roles.")]
            //public async Task badge_roles()
            //{
            //    await DeferAsync();

            //    foreach (var badge in Enum.GetValues(typeof(BotGuildRoleType)))
            //    {
            //        var role = Context.Guild.Roles.FirstOrDefault(a => a.Name.Contains(badge.ToString()));
            //        if(role is null)
            //    }
            //}
        }
    }
}
