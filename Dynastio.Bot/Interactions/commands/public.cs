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
            var latestRole = _rankedRoles[BotUser.activiy_level + 1];

            var message = await FollowupAsync(userMention,
                 embed: new EmbedBuilder()
                 {
                     Title = $"Level {BotUser.activiy_level}",
                     Description = $"Your level is **{BotUser.activiy_level}** and your xp: **{BotUser.activiy_score}**\n" +
                     $"You need **{RankService.getMax(BotUser.activiy_level) - BotUser.activiy_score}** more xp to get new role.",
                     Color = latestRole?.Color ?? Color.Orange,
                     Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Unlocked Roles")
                        .WithValue(string.Join(", ", rankedRoles.ToList().GetRange(0, BotUser.activiy_level).Select(a=> $"<@&{a}>")) + "\n.")
                        .WithIsInline(true)
                    },
                     ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                 }.Build());

        }
        [RateLimit(60)]
        [SlashCommand("leaderboard-rank", "user rank")]
        public async Task rleaderboard()
        {
            await DeferAsync();
            var leaderboard = await _userService.GetActivityScoreLeaderboardAsync(20);

            string names = string.Join("\n", leaderboard.Select(x => (leaderboard.IndexOf(x) + 1) + $". <@{x.Id}>"));
            string levels = string.Join("\n",leaderboard.Select(x => x.activiy_level) );
            string xps = string.Join("\n", leaderboard.Select(x => x.activiy_score.Metric()));


            var message = await FollowupAsync(userMention, embed:
                new EmbedBuilder()
                {
                    Title = "Top Active Users",
                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/1098332386674085988/1107719615678791781/circle_of_sacrifices_glow.png",
                    Color = Color.DarkGreen
                }
                .WithDescription("")
                .AddField("User", names, true)
                .AddField("Level  ", levels, true)
                .AddField(" XP", xps, true)
                .Build());
        }

    }

}
