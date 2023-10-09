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
using Dynastio.Bot;
using Dynastio.Bot.Global;

namespace Discord.Interactions
{
    public class RequireGuildOfficialAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Guild.Id != SavedGuilds.OfficialGuild)
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? ((CustomSocketInteractionContext)context).UserLocale["require.role.developer"]));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
