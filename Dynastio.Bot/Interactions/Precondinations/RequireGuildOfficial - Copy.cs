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
    public class RequireServerBoosterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context.User as IGuildUser).PremiumSince is not null)
            {
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["require.role.serverbooster"]));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
