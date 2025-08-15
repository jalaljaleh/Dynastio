using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions
{
    public abstract class BotPreconditionAttribute<TContext> : PreconditionAttribute where TContext : class, IInteractionContext
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context is not TContext typed)
                return Task.FromResult(PreconditionResult.FromError($"This command only works in a {typeof(TContext).Name} context."));

            // Delegate to the strongly-typed method
            return CheckRequirementsAsync(typed, commandInfo, services);
        }

        // Replace the original abstract with a strongly-typed version
        protected abstract Task<PreconditionResult> CheckRequirementsAsync(TContext context, ICommandInfo commandInfo, IServiceProvider services);
    }
}
