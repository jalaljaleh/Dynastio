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
using MongoDB.Driver;
using Dynastio.Data;

namespace Dynastio.Bot.Interactions.Modules.Guild
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles | ChannelPermission.SendMessages)]
    [Group("private-server", "dyanstio private server")]
    public class PrivateServerModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }


        [RateLimit(120, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("items", "get private server items")]
        public async Task items(bool Newest = false,int NewestNumber = 10)
        {
            await DeferAsync();

            string content = "";

            var items = (ItemType[])Enum.GetValues(typeof(ItemType));
            if (Newest)
                items = items.TakeLast(NewestNumber).ToArray();

            var items_ = items.GroupBy(a => a.ToString()[0].ToString()).OrderBy(a => a.Key).ToList();
            string[] headers = items_.Select(a => a.Key).ToArray();
            foreach (var g in items_)
                content += $"**{g.Key}:** ```" + string.Join(", ", g.Select(a => $"{a.ToString()}")) + "```";

            await FollowupAsync(embed: content.ToEmbed(Newest ? "Newest Items" : "Items List"));
        }
        [RateLimit(120, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("entities", "get private server entities")]
        public async Task entities(bool Newest = false, int NewestNumber = 10)
        {
            await DeferAsync();

            string content = "";

            var entities = (EntityType[])Enum.GetValues(typeof(EntityType));
            if (Newest)
                entities = entities.TakeLast(NewestNumber).ToArray();

            var entitiesG = entities.GroupBy(a => a.ToString()[0].ToString()).OrderBy(a => a.Key).ToList();
            string[] headers = entitiesG.Select(a => a.Key).ToArray();
            foreach (var g in entitiesG)
                content += $"**{g.Key}:** ```" + string.Join(", ", g.Select(a => $"{a.ToString()}")) + "```";

            await FollowupAsync(embed: content.ToEmbed(Newest ? "Newest Entities" : "Entities List"));
        }
        [RateLimit(10, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("find", "get server information")]
        public async Task server(
                   [Autocomplete(typeof(SharedAutocompleteHandler.OnlinePrivateServersAutocompleteHandler))] string server,
                   DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dynastio = Dynastio[provider];
            var result = dynastio.OnlineServers.FirstOrDefault(a => a.GetHashCode().ToString() == server);
            if (result == null)
            {
                await FollowupAsync(embed: $"Server `{server}` not found.".ToWarnEmbed("not found"));
                return;
            }
            var teams = result.Players.GroupBy(a => a.Team);

            var content =
                $"**Label**: {result.Label.TrySubstring(20)}\n" +
                $"**Region**: {result.Region}\n" +
                $"**TopPlayerName** {result.TopPlayerName}\n" +
                $"**TopPlayerLevel**: {result.TopPlayerLevel}\n" +
                $"**TopPlayerScore**: {result.TopPlayerScore.Metric()}\n" +
                $"**ConnectionsLimit**: {result.ConnectionsLimit}\n" +
                $"**Teams Count**: {teams.Count()}\n" +
                $"**IsPrivate**: {result.IsPrivate}\n" +
                $"**Map**: {result.Map}\n" +
                $"**NewIo**: {result.NewIo}\n" +
                $"**CustomMode**: {result.CustomMode}\n" +
                $"**FrameDrop**: {result.FrameDrop}\n" +
                $"**GameMode**: {result.GameMode}\n" +
                $"**Lifetime**: {result.Lifetime}\n" +
                $"**LoadAvg**: {result.LoadAvg}\n" +
                $"**LoadMax**: {result.LoadMax}\n" +
                $"**ServerTime**: {result.ServerTime}\n" +
                $"**Version**: {result.Version}\n" +
                $"**PlayersCount**: {result.PlayersCount}\n" +
                $"**Players**: {string.Join(", ", result.Players.Select(a => a.Nickname.TrySubstring(16))).ToMarkdown()}" +
                $"**Teams**: {string.Join(", ", teams.Select(a => a.Key)).ToMarkdown()}" +
                $"";

            await FollowupAsync(embed: content.ToEmbed("Server " + server));
        }

    }
}
