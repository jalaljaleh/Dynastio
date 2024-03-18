using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Extenstions;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
{
    public class LeaderboardButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }

        public const string CustomId = "btn.dynastio.leaderboard";
        public static Emoji Emoji => new Emoji("🥇");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.leaderboard.label"],
                Style = ButtonStyle.Primary,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var content =
                GetText(Leaderboardscore.GetLeaderboard(dynastio.Leaderboardscore, LeaderboardScoreItemId.Month), "Main") +
                GetText(Leaderboardscore.GetLeaderboard(dynastio.Leaderboardscore, LeaderboardScoreItemId.Solo_Month), "Solo") +
                GetText(Leaderboardscore.GetLeaderboard(dynastio.Leaderboardscore, LeaderboardScoreItemId.PVP_Month), "PVP");


            await ModifyCurrentMessageAsync(Context.User.Mention, embed: content.ToEmbed(Context.UserLocale["leaderboard"]));
        }
        string GetText(Leaderboardscore list, string Header)
        {
            var leaderboardContent = list.Data.OrderBy(a => a.Score).ToList();

            return Header +

                leaderboardContent.ToStringTable(new[] { "#", Context.UserLocale["index"], Context.UserLocale["score"], Context.UserLocale["nickname"] },
                          a => leaderboardContent.IndexOf(a) < 3 ? "🏆" : "",
                          a => $"{(leaderboardContent.IndexOf(a) + 1).ToRegularCounter()}",
                          a => $"{a.Score.Metric()}",
                          a => $"{a.Nickname.RemoveLines()}")
                                .ToMarkdown();
        }
    }
}
