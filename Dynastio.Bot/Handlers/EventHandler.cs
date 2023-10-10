using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DnsClient.Protocol;
using Dynastio.Data;
using Dynastio.Bot.Managers;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Core.Authentication;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Dynastio.Bot.Interactions.modules.dynastio.toplistModule;
using Dynastio.Bot.Global;
using System.Data;
using System.Net.Mime;

namespace Dynastio.Bot.Handlers
{
    internal class EventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly GuildService _guildService;
        private readonly DynastioData _usersService;
        private readonly RankService _rankService;
        private readonly GraphicService _graphicService;
        private readonly DynastioClient _dynastioClient;
        private readonly RepeaterService _repeaterService;
        private readonly DynastioData _database;

        public EventHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _configuration = _services.GetRequiredService<Configuration>();
            _guildService = _services.GetService<GuildService>();
            _rankService = _services.GetService<RankService>();
            _usersService = _services.GetService<DynastioData>();
            _graphicService = _services.GetService<GraphicService>();
            _dynastioClient = _services.GetService<DynastioClient>();
            _repeaterService = _services.GetRequiredService<RepeaterService>();
            _database = _services.GetRequiredService<DynastioData>();

            _client.Ready += _client_Ready;

        }


        private async Task _client_Ready()
        {
            _repeaterService
                .AddAction(RefreshStatusChannel, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5));


            //_repeaterService
            //    .AddFunction(EidMubarakEvent(), TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            await _client.Guilds.First()
                .GetTextChannel(1109911020341837825)
                .SendMessageAsync("Ready !")
            .TryAsync();


        }


        private async Task RefreshStatusChannel()
        {
            var servers = _dynastioClient.OnlineServers;

            var status =
                 $"- ` {_dynastioClient.OnlineServers.Where(a => !a.IsPrivate).Count()} ` public servers & ` {_dynastioClient.OnlinePlayers.Where(a => !a.Parent.IsPrivate).Count()} ` players.\n" +
                 $"- ` {_dynastioClient.OnlineServers.Where(a => a.IsPrivate).Count()} ` private servers & ` {_dynastioClient.OnlinePlayers.Where(a => a.Parent.IsPrivate).Count()} ` Players.\n";

            var publicServers = servers.Where(a => !a.IsPrivate).OrderByDescending(a => a.TopPlayerScore).ToList().ToStringTable(new[] { "R", "server", "score", "players" },
                a => servers.IndexOf(a),
                a => a.Label.TryRemove(18),
                a => a.TopPlayerScore.Metric(),
                a => a.PlayersCount + "/" + a.ConnectionsLimit)
                .ToMarkdown();

            var privateServers = servers.Where(a => a.IsPrivate).ToList().ToStringTable(new[] { "R", "Server", "Link" },
               a => servers.IndexOf(a) + " |",
               a => a.Label.TryRemove(18) + " |",
               a => $"[[Join {a.PlayersCount}/{a.ConnectionsLimit}]](https://dynast.io/?direct={a.Ip}:{a.Port})");

            var details = 
                 $"- **Current Version**: {_dynastioClient.Version.CurrentVersion} [Download]({_dynastioClient.Version.DownloadUrl})";

            Embed[] embds = new Embed[]
            {
                status.ToEmbed($" ✦•···············• Status {DateTime.UtcNow.ToDiscordUnixTimestampFormat()} •···············•✦"),
                publicServers.ToEmbed("✦•··············• Public Servers •··············•✦"),
                privateServers.ToEmbed("✦•··············• Private Servers •··············•✦"),
                details.ToEmbed("✦•··················• More •··················•✦")
            };

            var channel = await _client.GetChannelAsync(1124036365613539408);
            await Utilities.ChannelUtilities.SendOrUpdateMessage((ITextChannel)channel, _client.CurrentUser.Id, "", embds);
        }

    }
}
