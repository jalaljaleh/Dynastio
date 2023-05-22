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
        [SlashCommand("find-player", "find player")]
        public async Task find(   
            string name,
              [MaxValue(60)] int take = 30,
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
                a.Nickname.Contains(name)
                ).OrderByDescending(a => a.Score).ToList();

             
            var players1 = players.Skip((page - 1) * take).Take(take).ToList();
            var content = players1.ToStringTable(new[] { "#", Context.UserLocale["server"], Context.UserLocale["score"], Context.UserLocale["level"], Context.UserLocale["team"], Context.UserLocale["nickname"] },
                a => players.IndexOf(a),
                a => a.Parent.Label.TrySubstring(16),
                a => a.Score.Metric(),
                a => a.Level.Metric(),
                a => a.Team.RemoveLines().TrySubstring(6),
                a => a.Nickname.RemoveLines().TrySubstring(12))
                .ToMarkdown();

            // var map = Map == Map.Enable ? _graphicService.GetMap(players1) : null;

            var embeds = null != null ?
                new Embed[] { content.ToEmbed(), "".ToEmbed(imageUrl: "attachment://map.jpeg") } :
                new Embed[] { content.ToEmbed() };

            var msg = // null != null ?
                      // await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embeds) :
                await FollowupAsync(Context.User.Id.ToUserMention(), embeds);
        }


    }
}
