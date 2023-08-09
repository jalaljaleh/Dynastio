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
               .AddAction(RefreshFeaturedVideosChannel, TimeSpan.FromMinutes(35));

            _repeaterService
                .AddAction(RefreshStatusChannel, TimeSpan.FromMinutes(10));


            //_repeaterService
            //    .AddFunction(EidMubarakEvent(), TimeSpan.FromHours(1), TimeSpan.FromHours(1));


            await _client.Guilds.First()
                .GetTextChannel(1109911020341837825)
                .SendMessageAsync("Ready !")
                .TryAsync();
        }

        private async Task RefreshFeaturedVideosChannel()
        {
            var channel = _client.Guilds.First().GetTextChannel(1136917780516585472);
            if (channel == null) return;

            var msgs = await channel.GetMessagesAsync()
                .FlattenAsync()
                .TryAsync();

            if (msgs.isSuccesful is false) return;

            List<IMessage> postsToDelete = msgs.result.Where(a => a.Source == MessageSource.Bot).ToList();

            var uploadedVideos = postsToDelete.Select(a => a.Content).ToList();
            foreach (var video in _dynastioClient.FeaturedVideos.OrderByDescending(a => a.ExpireAt))
            {
                var toDeletePost = postsToDelete.FirstOrDefault(a => a.Content.Contains(video.Url));
                if (toDeletePost != null)
                    postsToDelete.Remove(toDeletePost);

                if (uploadedVideos.Any(a => a.Contains(video.Url)))
                    continue;

                var msg = await channel.SendMessageAsync(
                    $"## ✦•··························• Dynast.io •··························•✦\n" +
                    $"\n### Expire {video.ExpireAt.ToDiscordUnixTimestampFormat()}" +
                    "\nUrl: " + video.Url +
                    "\nGroup: " + video.Group +
                    "\nPriority: " + video.Priority);

                await Task.Delay(80);

                await msg.AddReactionAsync(new Emoji("👍"));

                await Task.Delay(550);
            }

            var _ex_channel = _client.Guilds.First().GetTextChannel(1137030131970494524);
            foreach (var msg in postsToDelete)
            {
                var content = msg.Content.Replace("Expire", "Expired");
                var msg1 = await _ex_channel.SendMessageAsync(
                    content +
                    "\n### Likes: " + (msg.Reactions?.FirstOrDefault().Value.ReactionCount ?? 0));
                await Task.Delay(80);

                await msg1.CrosspostAsync();
                await Task.Delay(550);
            }

            await channel.DeleteMessagesAsync(postsToDelete);
        }
        private async Task RefreshStatusChannel()
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

            var channel = await _client.GetChannelAsync(1124036365613539408);
            await Utilities.ChannelUtilities.SendOrUpdateMessage((ITextChannel)channel, _client.CurrentUser.Id, msgContent);
        }

    }
}
