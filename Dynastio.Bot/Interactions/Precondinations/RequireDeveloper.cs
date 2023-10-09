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
using static Dynastio.Data.Guild;
using Dynastio.Bot.Global;

namespace Discord.Interactions
{
    public class RequireDeveloperAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (SavedUsers.Develoeprs.Contains(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["require.developer"]));

        }
    }
}
