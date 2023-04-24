using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot;
using Discord.WebSocket;
using Discord;
using Dynastio;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions
{
    public class RequireChannelSlowmode : PreconditionAttribute
    {
        public int value { get; }
        public RequireChannelSlowmode(int SlowModeInterval)
        {
            this.value = SlowModeInterval;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {

            if ((context.Channel as ITextChannel).SlowModeInterval >= value)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? $"this command require channel slowmode more than {value} seconds."));
        }
    }
}
