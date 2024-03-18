using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Enums;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
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
    public class PlayersButton : BotInteractionModuleBase
    {
        public const string CustomId = "btn.dynastio.players";
        public static Emoji Emoji => new Emoji("⚔️");
        public static ButtonBuilder GetButton(Locale locale, int playersCount)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.players.label", playersCount],
                Style = ButtonStyle.Primary,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }

        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }


        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            // var server = "";
            int page = 0;
            int take = 30;
            ToplistSortType sort = ToplistSortType.Score;

            var interaction = Context.Interaction as SocketMessageComponent;

            var players = dynastio.OnlinePlayers.Where(a => !a.Parent.IsPrivate).ToList() ?? null;
            if (players is null)
            {
                await ModifyCurrentMessageAsync(embed: "No any online server found.".ToEmbed("Not Found !", color: Color.Orange));
                return;
            }

            //players = players.Where(
            //    a =>
            //    a.Parent.Label.ToLower().Contains(server)
            //    ).ToList();

            players = sort switch
            {
                ToplistSortType.Score => players.OrderByDescending(a => a.Score).ToList(),
                ToplistSortType.Level => players.OrderByDescending(a => a.Level).ToList(),
                ToplistSortType.Nickname => players.OrderByDescending(a => a.Nickname).ToList(),
                ToplistSortType.Team => players.OrderByDescending(a => a.Team).ToList(),
                ToplistSortType.ServerName => players.OrderByDescending(a => a.Parent.Label).ToList(),
                ToplistSortType.Location => players.OrderByDescending(a => a.X * a.Y).ToList(),
                _ => players.OrderByDescending(a => a.Score).ToList()
            };

            var players1 = players.Skip((page - 1) * take).Take(take).ToList();
            var content = players1.ToStringTable(new[] { "#", Context.UserLocale["server"], Context.UserLocale["score"], Context.UserLocale["level"], Context.UserLocale["team"], Context.UserLocale["nickname"] },
                                    a => players.IndexOf(a),
                                    a => a.Parent.Label.TryRemove(16),
                                    a => a.Score.Metric(),
                                    a => a.Level.Metric(),
                                    a => a.Team.RemoveLines().TryRemove(6),
                                    a => a.Nickname.RemoveLines().TryRemove(12))
                .ToMarkdown() + "\n" +
                    advertisingService.GetInlineEmbedDescription();

            await ModifyCurrentMessageAsync(embed: content.ToEmbed(userLocale["btn.dynastio.players.label", players1.Count]));
        }

    }
}
