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

namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [Group("players", "online players")]
    public class PlayersModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }
        public UserService UserService { get; set; }

        [RateLimit(10, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("discord", "search for a list of discord players")]
        public async Task discord(
           [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
           Map Map = Map.Disable,
           int skip = 0,
           DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dyanstioProvider = Dynastio[provider];
            var players = dyanstioProvider.OnlinePlayers.ToList() ?? null;
            if (players == null)
            {
                await FollowupAsync(embed: "No any online server found.".ToWarnEmbed("Not Found !"));
                return;
            }
            players = players.Where(
                a =>
                a.IsDiscordAuth() &&
                a.Parent.Label.ToLower().Contains(server)
                ).Skip(skip).Take(20).ToList();

            var content = players.ToStringTable(new[] { "#", this["server"], this["score"], this["level"], this["team"], this["nickname"] },
                a => players.IndexOf(a) + skip,
                a => a.Parent.Label.TrySubstring(16),
                a => a.Score.Metric(),
                a => a.Level.Metric(),
                a => a.Team.RemoveLines().TrySubstring(10),
                a => a.Nickname.RemoveLines().TrySubstring(16)).ToMarkdown();

            string content1 = string.Join("\n", players.Select(a => $"**{players.IndexOf(a).ToRegularCounter()}** <@{a.Id.Remove("discord:")}>"));

            var map = Map == Map.Enable ? GraphicService.GetMap(players) : null;


            var embeds = map != null ?
                new Embed[] { content.ToEmbed(), content1.ToEmbed(imageUrl: "attachment://map.jpeg") } :
                new Embed[] { content.ToEmbed(), content1.ToEmbed() };

            var msg = map != null ?
                await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embeds) :
                await FollowupAsync(Context.User.Id.ToUserMention(), embeds);
        }

        [RateLimit(10, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("search", "search for a list of online players")]
        public async Task players(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
            SortType sort = SortType.Score,
            Map Map = Map.Disable,
            [MaxValue(50)] int take = 25,
            string nickname = "",
            int MinLevel = 0,
            int MaxLevel = int.MaxValue,
            int MinScore = 0,
            int MaxScore = int.MaxValue,
            int page = 1,
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dyanstioProvider = Dynastio[provider];
            var players = dyanstioProvider.OnlinePlayers.ToList() ?? null;
            if (players == null)
            {
                await FollowupAsync(embed: "No any online server found.".ToWarnEmbed("Not Found !"));
                return;
            }
            players = players.Where(
                a =>
                a.Nickname.ToLower().Contains(nickname) &&
                a.Parent.Label.ToLower().Contains(server) &&
                a.Level > MinLevel && a.Level < MaxLevel &&
                a.Score > MinScore && a.Score < MaxScore
                ).ToList();

            switch (sort)
            {
                case SortType.Score:
                    players = players.OrderByDescending(a => a.Score).ToList();
                    break;
                case SortType.Level:
                    players = players.OrderByDescending(a => a.Level).ToList();
                    break;
                case SortType.Team:
                    players = players.OrderByDescending(a => a.Team).ToList();
                    break;
                case SortType.Server:
                    players = players.OrderByDescending(a => a.Parent.Label).ToList();
                    break;
                case SortType.Nickname:
                    players = players.OrderByDescending(a => a.Nickname).ToList();
                    break;
                case SortType.Region:
                    players = players.OrderByDescending(a => a.Parent.Region).ToList();
                    break;
            }
            var players1 = players.Skip((page - 1) * take).Take(take).ToList();
            var content = players1.ToStringTable(new[] { "#", this["server"], this["score"], this["level"], this["team"], this["nickname"] },
                a => players.IndexOf(a),
                a => a.Parent.Label.TrySubstring(16),
                a => a.Score.Metric(),
                a => a.Level.Metric(),
                a => a.Team.RemoveLines().TrySubstring(10),
                a => a.Nickname.RemoveLines().TrySubstring(16)).ToMarkdown();

            string content1 = this["total_servers:*", dyanstioProvider.OnlineServers.Count] + " | " +
                              this["total_players:*", dyanstioProvider.OnlinePlayers.Count] + "\n" +
                              this["page:*", $"{page}/{players.Count() / take}"].ToBold() + "\n" +
                              this["closes:*", 50.ToDiscordUnixTimeFromat()];

            var map = Map == Map.Enable ? GraphicService.GetMap(players1) : null;


            var embeds = map != null ?
                new Embed[] { content.ToEmbed(), "".ToEmbed(imageUrl: "attachment://map.jpeg") } :
                new Embed[] { content.ToEmbed() };

            var msg = map != null ?
                await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embeds) :
                await FollowupAsync(Context.User.Id.ToUserMention(), embeds);
        }



    }
}
