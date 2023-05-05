/*!
 * Dynastio  (https://github.com/jalaljaleh/Dynastio/)
 * Copyright 2022-2023 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Dynastio/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Dynastio/blob/master/Dynastio.Bot/Interactions/Precondinations/RequireBotOwner.cs)
 */

using Dynastio.Bot.Interactions;

namespace Discord.Interactions
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            IApplication application = await context.Client.GetApplicationInfoAsync();
            if (
                context.User.Id == application.Owner.Id ||
                application.Team != null &&
                application.Team.TeamMembers.Select(a => a.User.Id).Contains(context.User.Id)){
                return PreconditionResult.FromSuccess();
            }
            return PreconditionResult.FromError(((CustomSocketInteractionContext)context).UserLocale["access.denied.owner"]);
        }
    }
}
