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
            var leaderboard = await _userService.GetActivityScoreLeaderboardAsync(15);

            string counter = string.Join("\n.", leaderboard.Select(x => leaderboard.IndexOf(x)));
            string names = string.Join("\n", leaderboard.Select(x => $"<@{x.Id}>"));
            string scores = string.Join("\n", leaderboard.Select(x => $"**{x.activiy_score}**"));


            var message = await FollowupAsync(userMention, embed:
                new EmbedBuilder()
                .AddField("R", counter, true)
                .AddField("Name", names, true)
                .AddField("Score", scores, true)
                .Build());
        }

    }

}
