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
            var accounts = (context as ICustomInteractionContext).BotUser.Accounts;
            if (accounts is null || accounts.Count == 0)
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((ICustomInteractionContext)context).Locale["require_account"]));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
