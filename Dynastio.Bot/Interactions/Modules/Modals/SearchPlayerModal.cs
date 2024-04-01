using Discord;
using Discord.Interactions;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Modals.Forms;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Extenstions;

namespace Dynastio.Bot.Interactions.Modules.Modals
{
    public class SearchPlayerModal : BotInteractionModuleBase
    {
        public const string CustomId = "modal.dynastio.players.search";


        public DynastioApi dynastio { get; set; }

        [ModalInteraction(CustomId)]
        public async Task Execute(SearchPlayerModalForm modal)
        {
            await DeferAsync();

            var players = dynastio.OnlinePlayers
                .Where(a =>
                        a.IsMatched(modal.PlayerNickname ?? "") &&
                        a.Parent.IsMatched(modal.Server ?? "") &&
                        a.Parent.IsPrivate == false)
                .ToList();

            if (int.TryParse(modal.PlayerLevel, out var level))
                players = players.Where(a => a.Level == level).ToList();

            if (int.TryParse(modal.PlayerScore, out var score))
                players = players.Where(a => a.Score == score).ToList();

            players = players.OrderByDescending(a => a.Score).Take(20).ToList();

            var content = players.ToStringTable(new[] { "#", Context.UserLocale["server"], Context.UserLocale["score"], Context.UserLocale["level"], Context.UserLocale["team"], Context.UserLocale["nickname"] },
                                   a => players.IndexOf(a),
                                   a => a.Parent.Label.TryRemove(16),
                                   a => a.Score.Metric(),
                                   a => a.Level.Metric(),
                                   a => a.Team.RemoveLines().TryRemove(6),
                                   a => a.Nickname.RemoveLines().TryRemove(12))
                   .ToMarkdown() + "\n";

            string details =
                (
                $"\n" +
                $"-{Context.UserLocale["nickname"]}: {modal.PlayerNickname}" +
                $" -{Context.UserLocale["server"]}: {modal.Server}" +
                $" -{Context.UserLocale["score"]}: {modal.PlayerScore}" +
                $" -{Context.UserLocale["level"]}: {modal.PlayerLevel}" +
                $" -{Context.UserLocale["team"]}: {modal.Team}" +
                $"\n" +
                $"-c {dynastio.OnlinePlayers.Count} players, {players.Count} result, {dynastio.OnlineServers.Count} servers" +
                $"").ToMarkdown() + "\n" + advertisingService.GetInlineEmbedDescription();

            await ModifyCurrentMessageAsync(
                text: userMention + " " + userLocale["modal.dynastio.searchplayer.title", players.Count] + "\n" + content,
                embed: details.ToEmbed(thumbnailUrl: BotAvatarUrl));
        }

    }
}
