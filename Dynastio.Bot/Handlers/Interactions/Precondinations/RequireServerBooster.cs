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
    public class RequireServerBoosterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(Discord.IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context.User as IGuildUser).PremiumSince is null)
            {
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((BotSocketInteractionContext)context).UserLocale["require.role.serverbooster"]));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
