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
        [SlashCommand("rank-leaderboard", "user rank")]
        public async Task syncroles()
        {
            await DeferAsync();
            var leaderboard = await _userService.GetActivityScoreLeaderboardAsync(20);

            string names = string.Join("\n", leaderboard.Select(x => (leaderboard.IndexOf(x) + 1) +  $". <@{x.Id}>"));
            string levels = string.Join("\n", leaderboard.Select(x => x.activiy_level));
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
                .AddField("Level", levels, true)
                .AddField("XP", xps, true)
                .Build());
        }

    }

}
