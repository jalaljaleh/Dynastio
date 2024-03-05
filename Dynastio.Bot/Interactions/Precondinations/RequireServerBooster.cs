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
    public class RequireServerBoosterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context.User as IGuildUser).PremiumSince is null)
            {
                return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? ((BotSocketInteractionContext)context).UserLocale["require.role.serverbooster"]));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
