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
    public class RequireRankingLevelAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        public int Level { get; set; }
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.BotGuild.RankingSettings.IsEnabled is false)
                return Task.FromResult(PreconditionResult.FromError("ranking module is disabled by admin"));

            var userLevel = context.BotUser.TryGetGuildProfile(context.Guild.Id);
            if (userLevel.Level < Level)
                return Task.FromResult(PreconditionResult.FromError($"Level {Level} required for this command and you are level {userLevel.Level} !"));

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }


}
