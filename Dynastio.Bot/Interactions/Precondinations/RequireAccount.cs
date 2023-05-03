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

namespace Discord.Interactions
{
    public class RequireUserDynastioAccountAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context as CustomSocketInteractionContext).BotUser.GetDefaultAccount() is null)
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["accounts.require.default"]));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
