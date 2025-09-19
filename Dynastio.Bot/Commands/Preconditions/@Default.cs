using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands
{
    public class RequireBotTeamAttribute : BotPreconditionAttribute<BotSocketCommandContext>
    {
        /// <inheritdoc />
        public override string ErrorMessage { get; set; }

        /// <inheritdoc />

        protected override async Task<PreconditionResult> CheckRequirementsAsync(BotSocketCommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (false)
                return await Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? ""));

            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
