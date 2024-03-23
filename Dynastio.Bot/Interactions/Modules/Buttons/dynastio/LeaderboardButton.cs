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
using System.Runtime.CompilerServices;
using Dynastio.Bot.Interactions.Modules.shared_buttons;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
{
    public class LeaderboardButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }

        public const string CustomId = "btn.dynastio.leaderboard:*";
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
                CustomId = CustomId.Replace("*", "select")
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync(string arg)
        {
            await DeferAsync();

            if (arg == "select")
            {
                var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.EmbedMessageContent, 1).FirstOrDefault();

                var cb = new ComponentBuilder()
                    .WithButton("Daily", CustomId.Replace("*", "0"), ButtonStyle.Success, new Emoji("👑"), null, false, 0)
                    .WithButton("Weekly", CustomId.Replace("*", "1"), ButtonStyle.Success, new Emoji("👑"), null, false, 0)
                    .WithButton("Monthly", CustomId.Replace("*", "2"), ButtonStyle.Success, new Emoji("👑"), null, false, 0)

                    .WithButton("Solo - Daily", CustomId.Replace("*", "3"), ButtonStyle.Primary, new Emoji("🤴"), null, false, 1)
                    .WithButton("Solo - Weekly", CustomId.Replace("*", "4"), ButtonStyle.Primary, new Emoji("🤴"), null, false, 1)
                    .WithButton("Solo - Monthly", CustomId.Replace("*", "5"), ButtonStyle.Primary, new Emoji("🤴"), null, false, 1)

                    .WithButton("PvP - Daily", CustomId.Replace("*", "6"), ButtonStyle.Secondary, new Emoji("⚔️"), null, false, 2)
                    .WithButton("PvP - Weekly", CustomId.Replace("*", "7"), ButtonStyle.Secondary, new Emoji("⚔️"), null, false, 2)
                    .WithButton("PvP - Monthly", CustomId.Replace("*", "8"), ButtonStyle.Secondary, new Emoji("⚔️"), null, false, 2)

                    .WithButton(CancelButton.GetButton(userLocale),3)
                    .Build();

                await ModifyCurrentMessageAsync(userMention + " " + contentAdvertises?.GetEmbedLink(), components: cb);

            }
            else
            {
                LeaderboardScoreItemId type = (LeaderboardScoreItemId)int.Parse(arg);
                var content = GetText(Leaderboardscore.GetLeaderboard(dynastio.Leaderboardscore, type), type.ToString());

                await ModifyCurrentMessageAsync(Context.User.Mention, embed: content.ToEmbed(Context.UserLocale["leaderboard"] +" "+ type));
            }

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
