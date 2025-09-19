using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions
{
    /// <summary>
    ///     Requires the command to be invoked by a member of the team that owns the bot.
    /// </summary>
    /// <remarks>
    ///     This precondition will restrict the access of the command or module to the a member of the team of the Discord application, narrowed to specific team roles if specified.
    ///     If the precondition fails to be met, an erroneous <see cref="PreconditionResult"/> will be returned with the
    ///     message "Command can only be run by a member of the bot's team."
    ///     <note>
    ///     This precondition will only work if the account has a <see cref="TokenType"/> of <see cref="TokenType.Bot"/>
    ///     ;otherwise, this precondition will always fail.
    ///     </note>
    /// </remarks>

    public class RequireApplicationTeamAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        /// <summary>
        ///      The team roles to require. Valid values: "*", "admin", "developer", or "read_only"
        /// </summary>
        public string[] TeamRoles { get; } = [];

        /// <summary>
        ///     Requires that the user invoking the command to have a specific team role.
        /// </summary>
        /// <param name="teamRoles">The team roles to require. Valid values: "*", "admin", "developer", or "read_only"</param>
        public RequireApplicationTeamAttribute(params string[] teamRoles)
        {
            TeamRoles = teamRoles ?? TeamRoles;
        }

        /// <inheritdoc />
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.ClientService.HasTeamRole(context.User.Id, TeamRoles))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"Command can only be run by a member of the bot's team {(TeamRoles.Length == 0 ? "." : "with the specified permissions.")}"));
        }


    }
}
