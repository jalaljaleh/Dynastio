using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Shard;
using System.ComponentModel;

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{
    public partial class DynastioModule
    {
        [SlashCommand("toplist", "a list of top players")]
        public async Task toplist(
              [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
              [MaxValue(60)] int take = 30,
              ToplistSortType sort = ToplistSortType.Score,
              int page = 1)
        {
            await DeferAsync();

            var players = _dynastio.OnlinePlayers.Where(a => !a.Parent.IsPrivate).ToList() ?? null;
            if (players == null)
            {
                await FollowupAsync(embed: "No any online server found.".ToEmbed("Not Found !", color: Color.Orange));
                return;
            }
            players = players.Where(
                a =>
                a.Parent.Label.ToLower().Contains(server)
                ).ToList();

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
                .ToMarkdown();

            // var map = Map == Map.Enable ? _graphicService.GetMap(players1) : null;

            var embeds = null != null ?
                new Embed[] { content.ToEmbed(), "".ToEmbed(imageUrl: "attachment://map.jpeg") } :
                new Embed[] { content.ToEmbed() };

            var msg = // null != null ?
                      // await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embeds) :
                await FollowupAsync(Context.User.Id.ToUserMention(), embeds);
        }

        public enum ToplistSortType
        {
            Score,
            Level,
            Nickname,
            Team,
            Location,
            [ChoiceDisplay("Server Name")]
            ServerName
        }

    }
}
