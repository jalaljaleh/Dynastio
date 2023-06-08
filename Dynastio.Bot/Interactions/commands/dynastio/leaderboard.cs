using Discord;
using Discord.Interactions;
using Dynastio.Bot.Data;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.commands.dynastio
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RateLimit(4)]
    [Group("leaderboard", "Leaderboards")]
    public class leaderboardModule : CustomInteractionModuleBase
    {
        public DynastioClient _dynastio { get; set; }
        public UserService _userService { get; set; }

        [SlashCommand("rank", "user rank")]
        public async Task rank()
        {
            await DeferAsync();
            var leaderboard = await _userService.GetActivityScoreLeaderboardAsync(15);

            string names = string.Join("\n", leaderboard.Select(x => (leaderboard.IndexOf(x) + 1) + $". <@{x.Id}>"));
            string levels = string.Join("\n", leaderboard.Select(x =>  x.activiy_level.ToString().ToMarkdown()));
            string xps = string.Join("\n", leaderboard.Select(x =>  x.activiy_score.Metric().ToString().ToMarkdown()));


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

        [SlashCommand("score", "leaderboard score")]
        public async Task svore(LeaderboardType leaderboard = LeaderboardType.Monthly)
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
        [SlashCommand("coin", "leaderboard coin")]
        public async Task coin()
        {
            await DeferAsync();

            var coinboard = _dynastio.Leaderboardcoins;
            string content = coinboard.ToStringTable(new[] { "#", Context.UserLocale["index"], Context.UserLocale["coin"], Context.UserLocale["nickname"] },
                 a => coinboard.IndexOf(a) < 5 ? "🏆" : "",
                 a => $"{(coinboard.IndexOf(a) + 1).ToRegularCounter()}",
                 a => $"{a.Coin.Metric()}",
                 a => $"{getUsernameAsync(a.Id).Result}");

            await FollowupAsync(Context.User.Id.ToUserMention(), embed: content.ToMarkdown().ToEmbed(Context.UserLocale["leaderboard"] + " " + Context.UserLocale["coin"]));
        }
        public async Task<string> getUsernameAsync(string id)
        {
            if (!id.Contains("discord:"))
                return "Hidden";
            ulong _id = ulong.Parse(id.Replace("discord:", ""));
            var user = await Context.Client.GetUserAsync(_id);
            return user?.Username ?? "Unknown";
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
