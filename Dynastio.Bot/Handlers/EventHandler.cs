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
            var servers = _dynastioClient.OnlineServers.Where(a => a.IsPrivate == false).OrderByDescending(a => a.TopPlayerScore).Take(17).ToList();
            var content = servers.ToStringTable(new[] { "#", "server", "score", "level", "nickname" },
                a => servers.IndexOf(a),
                a => a.Label.TryRemove(18),
                a => a.TopPlayerScore.Metric(),
                a => a.TopPlayerLevel.Metric(),
                a => a.TopPlayerName.RemoveLines().TryRemove(18))
                .ToMarkdown();

            var privateServers = servers.Where(a=>a.IsPrivate).ToStringTable(new[] { "#", "server", "top player", "players count","Link" },
               a => servers.IndexOf(a),
               a => a.Label.TryRemove(18),
               a => a.TopPlayerName.RemoveLines().TryRemove(16),
               a => a.PlayersCount,
               a=> $"[Join](https://dynast.io/?direct={a.Ip}:{a.Port})");

            var msgContent =
                 $"## ✦•···············• Status {DateTime.UtcNow.ToDiscordUnixTimestampFormat()} •···············•✦\n" +

                 $"- ` {_dynastioClient.OnlineServers.Where(a => !a.IsPrivate).Count()} ` public servers & ` {_dynastioClient.OnlinePlayers.Where(a => !a.Parent.IsPrivate).Count()} ` players.\n" +
                 $"- ` {_dynastioClient.OnlineServers.Where(a => a.IsPrivate).Count()} ` private servers & ` {_dynastioClient.OnlinePlayers.Where(a => a.Parent.IsPrivate).Count()} ` Players.\n" +

                 $"\n{content}\n" +

                 $"### ✦•··············• Private Servers •··············•✦\n" +
                 $"\n{privateServers}\n" +

                 $"### ✦•··················• More •··················•✦\n" +
                 $"- **Current Version**: {_dynastioClient.Version.CurrentVersion} [Download]({_dynastioClient.Version.DownloadUrl})\n" +

                 $"";

            var channel = await _client.GetChannelAsync(1124036365613539408);
            await Utilities.ChannelUtilities.SendOrUpdateMessage((ITextChannel)channel, _client.CurrentUser.Id, msgContent);
        }

    }
}
