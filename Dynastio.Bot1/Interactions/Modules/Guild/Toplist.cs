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
    public class ToplistModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }

        [RateLimit(8, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("toplist", "a list of top players")]
        public async Task toplist(
              [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
              [MaxValue(60)] int take = 30,
              ToplistSortType sort = ToplistSortType.Score,
              [Summary("Map", "Display The Mini Map")] Map Map = Map.Disable,
               int page = 1,
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            var players = Dynastio[provider].OnlinePlayers.Where(a => !a.Parent.IsPrivate).ToList() ?? null;
            if (players == null)
            {
                await FollowupAsync(embed: "No any online server found.".ToWarnEmbed("Not Found !"));
                return;
            }
            players = players.Where(
                a =>
                a.Parent.Label.ToLower().Contains(server)
                ).ToList();

            switch (sort)
            {
                default:
                case ToplistSortType.Score:
                    players.OrderByDescending(a => a.Score).ToList();
                    break;

                case ToplistSortType.Level:
                    players.OrderByDescending(a => a.Level).ToList();
                    break;
                case ToplistSortType.Nickname:
                    players.OrderByDescending(a => a.Nickname).ToList();
                    break;
                case ToplistSortType.Team:
                    players.OrderByDescending(a => a.Team).ToList();
                    break;
                case ToplistSortType.Location:
                    players.OrderByDescending(a => a.X * a.Y).ToList();
                    break;
                case ToplistSortType.ServerNickname:
                    players.OrderByDescending(a => a.Parent.Label).ToList();
                    break;
            }


            var players1 = players.Skip((page - 1) * take).Take(take).ToList();
            var content = players1.ToStringTable(new[] { "#", this["server"], this["score"], this["level"], this["team"], this["nickname"] },
                a => players.IndexOf(a),
                a => a.Parent.Label.TrySubstring(16),
                a => a.Score.Metric(),
                a => a.Level.Metric(),
                a => a.Team.RemoveLines().TrySubstring(10),
                a => a.Nickname.RemoveLines().TrySubstring(16))
                .ToMarkdown();

            var map = Map == Map.Enable ? GraphicService.GetMap(players1) : null;

            var embeds = map != null ?
                new Embed[] { content.ToEmbed(), "".ToEmbed(imageUrl: "attachment://map.jpeg") } :
                new Embed[] { content.ToEmbed() };

            var msg = map != null ?
                await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embeds) :
                await FollowupAsync(Context.User.Id.ToUserMention(), embeds);
        }

        public enum ToplistSortType
        {
            Score,
            Level,
            Nickname,
            Team,
            Location,
            [ChoiceDisplay("Server Nickname")]
            ServerNickname
        }

    }
}
