using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands
{
    public abstract class BotPreconditionAttribute<TContext> : PreconditionAttribute where TContext : class, ICommandContext
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context is not TContext typed)
                return Task.FromResult(PreconditionResult.FromError($"This command only works in a {typeof(TContext).Name} context."));

            // Delegate to the strongly-typed method
            return CheckRequirementsAsync(typed, command, services);
        }

        // Replace the original abstract with a strongly-typed version
        protected abstract Task<PreconditionResult> CheckRequirementsAsync(TContext context, CommandInfo command, IServiceProvider services);
    }
}
