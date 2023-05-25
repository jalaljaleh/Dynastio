using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{
    public partial class DynastioModule
    {
        [SlashCommand("leaderboard-score", "leaderboard score")]
        public async Task LeaderboardScore(LeaderboardType leaderboard = LeaderboardType.Monthly)
        {
            await DeferAsync();

            var leaderboardContent = leaderboard switch
            {
                LeaderboardType.Monthly => _dynastio.LeaderboardscoresMonthly,
                LeaderboardType.Weekly => _dynastio.LeaderboardscoresWeekly,
                LeaderboardType.Daily => _dynastio.LeaderboardscoresDaily,
                _ => throw new NotImplementedException(),
            };
            leaderboardContent = leaderboardContent.OrderByDescending(a => a.Score).ToList();

            string content = leaderboardContent.ToStringTable(new[] { "#", Context.UserLocale["index"], Context.UserLocale["score"], Context.UserLocale["time"], Context.UserLocale["nickname"] },
                a => leaderboardContent.IndexOf(a) < 3 ? "🏆" : "",
                a => $"{(leaderboardContent.IndexOf(a) + 1).ToRegularCounter()}",
                a => $"{a.Score.Metric()}",
                a => a.CreatedAt.ToRelative(),
                a => $"{a.Nickname.RemoveLines()}");
            await FollowupAsync(Context.User.Id.ToUserMention(), embed: content.ToMarkdown().ToEmbed(Context.UserLocale["leaderboard"] + " " + Context.UserLocale[leaderboard.ToString().ToLower()]));
        }
        [SlashCommand("leaderboard-coin", "leaderboard coin")]
        public async Task leaderboard_coin()
        {
            await DeferAsync();

            var coinboard = _dynastio.Leaderboardcoins;
            string content = coinboard.ToStringTable(new[] { "#", Context.UserLocale["index"], Context.UserLocale["coin"], Context.UserLocale["nickname"] },
                 a => coinboard.IndexOf(a) < 5 ? "🏆" : "",
                 a => $"{(coinboard.IndexOf(a) + 1).ToRegularCounter()}",
                 a => $"{a.Coin.Metric()}",
                 a => $"{a.Name.RemoveLines()}");

            await FollowupAsync(Context.User.Id.ToUserMention(), embed: content.ToMarkdown().ToEmbed(Context.UserLocale["leaderboard"] + " " + Context.UserLocale["coin"]));
        }
        //[RateLimit(10)]
        //[SlashCommand("honor", "leaderboard honor")]
        //public async Task leaderboard_honor()
        //{
        //    await DeferAsync();

        //    var top10Honor = await UserService.Get10TopHonor();
        //    string content = top10Honor.ToStringTable(new[] { "#", this["index"], this["honor"], this["nickname"] },
        //        a => top10Honor.IndexOf(a) < 5 ? "🏆" : "",
        //        a => $"{(top10Honor.IndexOf(a) + 1).ToRegularCounter()}",
        //        a => $"{a.Honor.Metric()}",
        //        a => $"<@{a.Id}>");

        //    await FollowupAsync(Context.User.Id.ToUserMention(), embed: content.ToEmbed(this["leaderboard"] + " " + this["honor"]));
        //}
    }
}
