using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;

namespace Dynastio.Bot.Interactions.Precondinations
{
    public class RequireTemplateAttributeAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {

            return Task.FromResult(PreconditionResult.FromError(ErrorMessage));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
    

}
