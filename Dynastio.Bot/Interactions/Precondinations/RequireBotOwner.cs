/*!
 * Dynastio  (https://github.com/jalaljaleh/Dynastio/)
 * Copyright 2022-2023 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Dynastio/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Dynastio/blob/master/Dynastio.Bot/Interactions/Precondinations/RequireBotOwner.cs)
 */

using Discord;
using Discord.Interactions;

namespace Dynastio.Bot.Interactions.Precondinations
{
    public class RequireTeamMemberAttribute : PreconditionAttribute
    {
        IApplication _application;
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (_application is null)
                _application = await context.Client.GetApplicationInfoAsync();

            if (
                context.User.Id == _application.Owner.Id ||
                _application.Team != null &&
                _application.Team.TeamMembers.Select(a => a.User.Id).Contains(context.User.Id))
            {
                return PreconditionResult.FromSuccess();
            }
            return PreconditionResult.FromError(((BotSocketInteractionContext)context).UserLocale["access_denied"]);
        }
    }
}
