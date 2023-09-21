using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Data;
using Dynastio.Bot.Services;

using System.Net.Http.Headers;

namespace Dynastio.Bot.Interactions.modules
{

    [RequireGuildOfficial]
    [RequireContext(ContextType.Guild)]
    public class Leaderboard : CustomInteractionModuleBase
    {
        public DynastioData _dynastioData { get; set; }
        public DynastioData _database { get; set; }

        [RateLimit(60)]
        [SlashCommand("rank", "user rank")]
        public async Task rank()
        {
            await DeferAsync();

            var _rankedRoles = Context.Guild.Roles
                           .Where(x => x.Name.StartsWith("rank: "))
                           .OrderBy(a => a.Position)
                           .ToList();

            var rankedRoles = _rankedRoles.Select(a => a.Id);
            SocketRole latestRole = _rankedRoles[BotUser.activiy_level - 1] ?? null;
            SocketRole nextRole = _rankedRoles[BotUser.activiy_level + 1] ?? null;

            var isServerBooster = Context.User as IGuildUser is not { PremiumSince: null };
            var reachableXp = RankService.GetReachableMessageXp(Context.User as IGuildUser, Context.BotUser);

            var message = await FollowupAsync(userMention,
                 embed: new EmbedBuilder()
                 {
                     Title = $"Level {BotUser.activiy_level}",
                     Description = $"Your are level **{BotUser.activiy_level}** and {latestRole?.Mention ?? "none"} ranked role.",
                     Color = latestRole?.Color ?? Color.Orange,
                     Fields = new List<EmbedFieldBuilder>()
                    {
                           new EmbedFieldBuilder()
                        .WithName("Current Status")
                        .WithValue(
                               $"Level: **{BotUser.activiy_level}**\n"+
                               $"Xp: **{BotUser.activiy_score}**\n"+
                               $"Role: **{latestRole.Mention ?? "none"}**\n"
                        )
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Unlocked Reward")
                        .WithValue( $"**{RankService.CalculateLevelReward(BotUser.activiy_level + 1)}** Coins")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Unlocked Role")
                        .WithValue( $"<@&{nextRole.Id}>")
                        .WithIsInline(true),

                          new EmbedFieldBuilder()
                        .WithName("New Level")
                        .WithValue($"You need **{RankService.RequiredXpToLevelUp(BotUser).Metric()}** more xp to get new level.")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Xp Details")
                        .WithValue(
                             $"**Message XP:** {RankService._score}\n"+
                             $"**Random:** ± {RankService._randomXp}\n"+
                             $"**Server Booster:** + {RankService.GetServerBoosterXp(Context.User as IGuildUser)}\n"+
                             $"**User Additive:** ± {BotUser.activiy_score_additive}\n"+
                             $"**User Achievements:** ± {RankService.GetServerAchievementsXp(Context.User as IGuildUser)}\n"+
                             $"**Reachable:** {reachableXp - RankService._randomXp} - {reachableXp + RankService._randomXp}\n"+
                             $"**User Xp Requirements:** {(RankService.HasXpRequirements(Context.User as IGuildUser) is true ? "Yes" : "No")}\n")
                        .WithIsInline(true),

                    },
                     ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                 }.Build());

        }


    }

}
