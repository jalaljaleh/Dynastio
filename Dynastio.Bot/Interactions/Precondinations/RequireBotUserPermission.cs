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
using Dynastio.Bot.Data;

namespace Discord.Interactions
{
    public class RequireBotUserPermissionAttribute : PreconditionAttribute
    {
        private readonly BotUserPermission _botUserPermissions;
        public RequireBotUserPermissionAttribute(BotUserPermission botUserPermissions)
        {
            _botUserPermissions = botUserPermissions;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context as CustomSocketInteractionContext).BotUser.HasPermission(_botUserPermissions))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["accounts.require.default"]));

        }
    }
}
