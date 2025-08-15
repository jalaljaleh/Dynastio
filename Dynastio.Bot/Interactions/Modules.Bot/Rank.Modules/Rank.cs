using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Bot.Rank.Modules
{
    [RequireContext(ContextType.Guild)]
    public class RankModule : BotInteractionModuleBase
    {
        [RequireRankingLevel(Level = 1)]
        [SlashCommand("rank", "description")]
        public async Task rank()
        {
            var profile = BotUser.GetServerProfile(Context.Guild.Id);

            var rankingRole = RoleHelper.GetLatestRoleStartWith(User as IGuildUser, BotGuild.XpSystemSettings.RankingRoleAssignmentPerfix);

            var embed = new EmbedBuilder()
            {
                Title = this["rank.embed.title", profile.Level],
                Description = this["rank.embed.description", new { profile.Level, profile.Xp }],
                ThumbnailUrl = rankingRole.GetIconUrl() ?? User.TryGetAvatarUrl() ?? Context.Client.CurrentUser.TryGetAvatarUrl(),
                Color = rankingRole?.Color ?? Color.Default
            }.Build();

            await RespondAsync(Context.User.Mention, embed: embed);
        }
    }


}
