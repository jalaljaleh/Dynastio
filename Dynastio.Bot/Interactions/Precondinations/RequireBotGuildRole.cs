using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Dynastio;
using Dynastio.Bot.Interactions;
using static Dynastio.Bot.Data.Guild;

namespace Discord.Interactions
{
    //public class RequireUserBotGuildRoleAttribute : PreconditionAttribute
    //{
    //    private BotGuildRoleType role;
    //    public RequireUserBotGuildRoleAttribute(BotGuildRoleType role)
    //    {
    //        this.role = role;
    //    }
    //    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    //    {
    //        var roleId = (context as CustomSocketInteractionContext).BotGuild.GetRoleId(role);

    //        if (roleId is 0 || !(context.User as IGuildUser).RoleIds.Contains(roleId))
    //        {
    //            return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["require.role.developer"]));
    //        }
    //        return Task.FromResult(PreconditionResult.FromSuccess());

    //    }
    //}
}
