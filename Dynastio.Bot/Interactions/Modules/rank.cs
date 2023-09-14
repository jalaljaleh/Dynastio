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
            int[] score = isServerBooster ? RankService._randomScoreServerBooster : RankService._randomScore;

            var message = await FollowupAsync(userMention,
                 embed: new EmbedBuilder()
                 {
                     Title = $"Level {BotUser.activiy_level}",
                     Description = $"Your are level **{BotUser.activiy_level}** You need **{RankService.getMax(BotUser.activiy_level) - BotUser.activiy_score}** more xp to get new level.",
                     Color = latestRole?.Color ?? Color.Orange,
                     Fields = new List<EmbedFieldBuilder>()
                    {
                           new EmbedFieldBuilder()
                        .WithName("Level")
                        .WithValue( $"Level: **{BotUser.activiy_level}**\n"+ $"Xp: **{BotUser.activiy_score.Metric()}**")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Level Reward")
                        .WithValue( $"**{RankService.CalculateReward(BotUser.activiy_level + 1)}** Coins")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Level Role")
                        .WithValue( $"<@&{nextRole.Id}>")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Accessible-Xp")
                        .WithValue($"{score[0]} - {score[1]}")
                        .WithIsInline(true),

                    },
                     ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                 }.Build());

        }


    }

}
