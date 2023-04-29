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
    [Group("server", "dyanstio server")]
    public class ServerModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [RateLimit(10, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("find", "get server information")]
        public async Task server(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server,
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dynastio = Dynastio[provider];
            var result = dynastio.OnlineServers.FirstOrDefault(a => a.Label.ToLower().Contains(server));
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
