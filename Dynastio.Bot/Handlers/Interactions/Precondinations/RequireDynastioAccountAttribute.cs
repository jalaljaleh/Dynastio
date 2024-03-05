using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dynastio;
using Discord.Interactions;
using Discord;

namespace Dynastio.Bot.Interactions
{
    public class RequireDynastioAccountAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context as BotSocketInteractionContext).BotUser.GetDefaultAccount() is null)
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((BotSocketInteractionContext)context).UserLocale["accounts.require.default"]));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
