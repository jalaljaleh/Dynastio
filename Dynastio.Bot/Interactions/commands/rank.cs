using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Services;
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules.@public
{

    [RequireGuildOfficial]
    public class Leaderboard : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }
        public IDynastioBotDatabase _database { get; set; }

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

            var isServerBooster =  Context.User as IGuildUser is not { PremiumSince: null };
            int[] score = isServerBooster ? RankService._randomScoreServerBooster : RankService._randomScore;

            var message = await FollowupAsync(userMention,
                 embed: new EmbedBuilder()
                 {
                     Title = $"Level {BotUser.activiy_level}",
                     Description = $"Your are level **{BotUser.activiy_level}**.\n" +
                     $"You need **{RankService.getMax(BotUser.activiy_level) - BotUser.activiy_score}** more xp to get new role.",
                     Color = latestRole?.Color ?? Color.Orange,
                     Fields = new List<EmbedFieldBuilder>()
                    {
                           new EmbedFieldBuilder()
                        .WithName("Level")
                        .WithValue( $"**{BotUser.activiy_level}**")
                        .WithIsInline(true),
                         
                         new EmbedFieldBuilder()
                        .WithName("XP")
                        .WithValue( $"**{BotUser.activiy_score.Metric()}**")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Next Level")
                        .WithValue( $"**{BotUser.activiy_level}** / <@&{nextRole.Id}>")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Accessible-Xp")
                        .WithValue($"{score[0]} - {score[1]}")
                        .WithIsInline(true),

                         new EmbedFieldBuilder()
                        .WithName("Unlocked Roles")
                        .WithValue(string.Join(", ", rankedRoles.ToList().GetRange(0, BotUser.activiy_level).Select(a=> $"<@&{a}>")) + "\n.")
                        .WithIsInline(false)
                    },
                     ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                 }.Build());

        }
        [RateLimit(60)]
        [SlashCommand("leaderboard-rank", "user rank")]
        public async Task rleaderboard()
        {
            await DeferAsync();
            var leaderboard = await _userService.GetActivityScoreLeaderboardAsync(15);

            string users = string.Join("\n", leaderboard.Select(x => (leaderboard.IndexOf(x) + 1) + $". <@{x.Id}>"));
            string levels = string.Join("\n",leaderboard.Select(x => $"**{x.activiy_level}**") );
            string xps = string.Join("\n", leaderboard.Select(x => $"**{x.activiy_score.Metric()}**"));


            var message = await FollowupAsync(userMention, embed:
                new EmbedBuilder()
                {
                    Title = "Top Active Users",
                    Color = Color.DarkGreen
                }
                .WithDescription("")
                .AddField("User", users, true)
                .AddField("Level  ", levels, true)
                .AddField(" XP", xps, true)
                .Build());
        }

    }

}
