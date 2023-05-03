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
    [Group("servers", "dyanstio server")]
    public class ServerModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [RateLimit(10, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("find", "get server information")]
        public async Task server(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server)
        {
            await DeferAsync();
            var result = Dynastio.OnlineServers.FirstOrDefault(a => a.Label.ToLower().Contains(server));
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
                .WithValue($"**Label**: {result.Label.TrySubstring(20)}" +
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
                .WithValue(string.Join(", ", result.Players.Select(a => a.Nickname.TrySubstring(16))).ToMarkdown())
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
