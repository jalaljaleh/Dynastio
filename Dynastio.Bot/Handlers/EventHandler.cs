using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DnsClient.Protocol;
using Dynastio.Bot.Data;
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

namespace Dynastio.Bot.Handlers
{
    internal class EventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly GuildService _guildService;
        private readonly UserService _usersService;
        private readonly RankService _rankService;
        private readonly GraphicService _graphicService;
        private readonly DynastioClient _dynastioClient;
        private readonly RepeaterService _repeaterService;
        private readonly IDynastioBotDatabase _database;

        public EventHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _configuration = _services.GetRequiredService<Configuration>();
            _guildService = _services.GetService<GuildService>();
            _rankService = _services.GetService<RankService>();
            _usersService = _services.GetService<UserService>();
            _graphicService = _services.GetService<GraphicService>();
            _dynastioClient = _services.GetService<DynastioClient>();
            _repeaterService = _services.GetRequiredService<RepeaterService>();
            _database = _services.GetRequiredService<IDynastioBotDatabase>();

            _client.Ready += _client_Ready;

        }


        private async Task _client_Ready()
        {
            if (Global.Main.IsDebug())
                return;

            _repeaterService
                .AddAction(status, TimeSpan.FromMinutes(10));

            //_repeaterService
            //    .AddFunction(EidMubarakEvent(), TimeSpan.FromHours(1), TimeSpan.FromHours(1));


            await _client.Guilds.First()
                .GetTextChannel(1109911020341837825)
                .SendMessageAsync("Ready !")
                .TryAsync();
        }
        public async Task status()
        {
            var channel = _client.Guilds
                .First()
                .GetTextChannel(1124036365613539408);

            if (channel == null) return;

            var msgs = await channel.GetMessagesAsync()
                .FlattenAsync()
                .TryAsync();


            if (msgs.isSuccesful)
                foreach (var message in msgs.result)
                {
                    if (message.Author.IsBot is false)
                        continue;

                    await sendMessage(message);
                }
            async Task sendMessage(IMessage msg = null)
            {
                var players = _dynastioClient.OnlinePlayers.Where(a => !a.Parent.IsPrivate).OrderByDescending(a => a.Score).Take(17).ToList();

                var topPlayer = players.FirstOrDefault();

                var tpmention = topPlayer.IsDiscordAuth ? "- <@" + topPlayer.Id.Replace("discord:", "") + ">" : "";

                var content = players.ToStringTable(new[] { "#", "server", "score", "level", "nickname" },
                    a => players.IndexOf(a),
                    a => a.Parent.Label.TryRemove(18),
                    a => a.Score.Metric(),
                    a => a.Level.Metric(),
                    a => a.Nickname.RemoveLines().TryRemove(18))
                    .ToMarkdown();

                var msgContent =
                     $"## Dynast.io Status {DateTime.UtcNow.ToDiscordUnixTimestampFormat()}\n\n" +

                     $"### Information \n" +
                     $"- **Current Version**: {_dynastioClient.Version.CurrentVersion} [Download]({_dynastioClient.Version.DownloadUrl})\n" +

                     $"### Servers and Players \n" +
                     $"- `{_dynastioClient.OnlineServers.Count}` servers and `{_dynastioClient.OnlinePlayers.Count}` players are online:\n" +
                     $" - ` {_dynastioClient.OnlineServers.Where(a => !a.IsPrivate).Count()} ` public servers & ` {_dynastioClient.OnlinePlayers.Where(a => !a.Parent.IsPrivate).Count()} ` players.\n" +
                     $" - ` {_dynastioClient.OnlineServers.Where(a => a.IsPrivate).Count()} ` private servers & ` {_dynastioClient.OnlinePlayers.Where(a => a.Parent.IsPrivate).Count()} ` Players.\n" +
                     $"\n" +
                 
                     $"\n{content}\n" +
                     $"";

                if (msg is null)
                {
                    await channel.SendMessageAsync(msgContent, allowedMentions: AllowedMentions.None);
                }
                else
                {
                    var res = await (msg as IUserMessage).ModifyAsync(x =>
                    {
                        x.Content = msgContent;
                    }).TryAsync();

                    if (res is false)
                    {
                        await msg.DeleteAsync();
                    }
                }
            }
        }
        async Task updateStatus()
        {
            int serversCount = _dynastioClient.OnlineServers
                .Where(a => a.IsPrivate == false)
                .Count();

            int playerscount = _dynastioClient.OnlinePlayers.Count;

            await _client.SetGameAsync($"{playerscount} players, {serversCount} Servers!", "", ActivityType.Watching);
        }

        public async Task EidMubarakEvent()
        {
            var code = await _database.GetRedeemCodeAsync(RedeemCode.RedeemType.Coin_100);
            if (code is null)
                return;

            var result = await _client.Guilds.First()
                .GetTextChannel(1108998382996946964)
                .SendMessageAsync(
                embed:
                ($"Here is a redeem code for ` Eid al-Adha `\n" +
                $"```{code.Code}```\nA code will be sent every 1 hour until 24 hours.").ToEmbed("Eid al-Adha", "https://cdn.discordapp.com/attachments/1098332386674085988/1123659712165056562/happy-eid-ul-adha-6.png", color: Discord.Color.Orange))
                .TryAsync();

            if (result.isSuccesful)
                await _database.DeleteAsync(code);

        }
    }
}
