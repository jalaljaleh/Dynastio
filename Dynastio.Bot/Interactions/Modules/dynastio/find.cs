using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.AutoCompeletes;

namespace Dynastio.Bot.Interactions.modules.dynastio
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RateLimit(4)]
    [Group("find", "Find commands")]
    public class FindModule : CustomInteractionModuleBase
    {
        public DynastioClient _dynastio { get; set; }

        [SlashCommand("player", "find player")]
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
               a.Nickname.Trim().ToLower()
               .Contains(name.ToLower().Trim())
                ).OrderByDescending(a => a.Score).ToList();


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


        [SlashCommand("server", "get server information")]
        public async Task server(
            [MaxLength(20), Autocomplete(typeof(AutoCompeleteOnlineServers))] string server)
        {
            await DeferAsync();
            var result = _dynastio.OnlineServers.FirstOrDefault(a => a.Label.ToLower().Contains(server));
            if (result == null)
            {
                await FollowupAsync(embed: $"Server `{server}` not found.".ToEmbed("not found", color: Color.Orange));
                return;
            }
            var teams = result.Players.GroupBy(a => a.Team);
            var embed = new EmbedBuilder()
            {
                Title = "Server " + server,
                Fields = new List<EmbedFieldBuilder>()
                {
                new EmbedFieldBuilder()
                .WithName("Server")
                .WithValue($"**Label**: {result.Label.TryRemove(20)}" +
                $"**Region**: {result.Region}\n" +
                $"**Players**: {result.PlayersCount}\n" +
                $"**Teams**: {teams.Count()}\n" +
                $"**Limit**: {result.ConnectionsLimit}")
                .WithIsInline(true),

                new EmbedFieldBuilder()
                .WithName("Top Player")
                .WithValue(
                    $"**Name** {result.TopPlayerName}\n" +
                    $"**Level**: {result.TopPlayerLevel}\n" +
                    $"**Score**: {result.TopPlayerScore.Metric()}"
                    ).WithIsInline(true),



                new EmbedFieldBuilder()
                .WithName("Details")
                .WithValue($"**IsPrivate**: {result.IsPrivate}\n" +
                $"**Map**: {result.Map}\n" +
                $"**NewIo**: {result.NewIo}\n" +
                $"**CustomMode**: {result.CustomMode}\n" +
                $"**GameMode**: {result.GameMode}\n" +
                $"**Lifetime**: {result.Lifetime}\n" +
                $"**deleteMessage**: {result.Version}")
                .WithIsInline(true),


                new EmbedFieldBuilder()
                .WithName("Players")
                .WithValue(string.Join(", ", result.Players.Select(a => a.Nickname.TryRemove(16))).ToMarkdown())
                .WithIsInline(false),

                new EmbedFieldBuilder()
                .WithName("Teams")
                .WithValue(string.Join(", ", teams.Select(a => a.Key)).ToMarkdown())
                .WithIsInline(true)
                }
            }.Build();

            await FollowupAsync(embed: embed);
        }

    }
}
