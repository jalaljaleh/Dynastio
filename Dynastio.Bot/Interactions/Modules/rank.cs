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
            var latestRole = _rankedRoles[BotUser.activiy_level - 1];
            var nextRole = _rankedRoles[BotUser.activiy_level + 1];

            var isServerBooster = Context.User as IGuildUser is not { PremiumSince: null };

            var message = await FollowupAsync(userMention,
                 embed: new EmbedBuilder()
                 {
                     Title = $"Level {BotUser.activiy_level}",
                     Description = $"Your are level **{BotUser.activiy_level}** You need **{RankService.RequiredXpToLevelUp(BotUser)}** more xp to get new level.",
                     Color = latestRole?.Color ?? Color.Orange,
                     Fields = new List<EmbedFieldBuilder>()
                    {
                           new EmbedFieldBuilder()
                        .WithName("Current Level")
                        .WithValue( $"Level: **{BotUser.activiy_level}**\n"+ $"Xp: **{BotUser.activiy_score.Metric()}**")
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
                        .WithName("Xp Details")
                        .WithValue(
                             $"Message XP: {RankService._score}\n"+
                             $"Random: ±{RankService._randomXp}\n"+
                             $"Server Booster: +{RankService.GetServerBoosterXp(Context.User as IGuildUser)}\n"+
                             $"User Additive: ±{BotUser.activiy_score_additive}\n"+
                             $"Reachable: {RankService.GetReachableMessageXp(Context.User as IGuildUser,Context.BotUser)}\n")
                        .WithIsInline(true),

                    },
                     ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                 }.Build());

        }


    }

}
