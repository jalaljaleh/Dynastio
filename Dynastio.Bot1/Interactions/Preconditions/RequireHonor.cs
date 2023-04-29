using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Dynastio.Bot.Interactions;

namespace Discord.Interactions
{
    public class RequireHonor : PreconditionAttribute
    {
        public int? Count
        {
            get;
        }
        public RequireHonor(int Count)
        {
            this.Count = Count;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if ((context as ICustomInteractionContext).BotUser.Honor >= Count)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("You need more honor ."));

        }


    }
}
