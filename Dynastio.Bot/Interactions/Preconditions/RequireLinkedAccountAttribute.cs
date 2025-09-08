using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;

namespace Dynastio.Bot.Interactions
{
    public class RequireLinkedAccountAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        public override string ErrorMessage => "You need to login to your game account.";
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.BotUser.HasLinkedAccount)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(ErrorMessage));

        }
    }


}
