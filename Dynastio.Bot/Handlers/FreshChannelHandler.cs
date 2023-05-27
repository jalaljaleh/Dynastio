using Discord;
using Discord.WebSocket;
using DnsClient.Protocol;
using Dynastio.Bot.Data;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot
{
    public class FreshChannelHandler
    {
        private readonly IServiceProvider _services;
        private readonly RepeaterService _repeaterService;
        private readonly DiscordSocketClient _discord;
        private readonly DynastioClient _dynastioClient;
        public FreshChannelHandler(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _dynastioClient = services.GetRequiredService<DynastioClient>();
            _repeaterService = services.GetRequiredService<RepeaterService>();
            _discord.Ready += _discord_Ready;
        }

        private async Task _discord_Ready()
        {
           // _repeaterService.AddFunction(UpdateChannels(), TimeSpan.FromMinutes(5));
        }

        public async Task UpdateChannels()
        {
            await UpdateOnlineServers();

            await Task.Delay(200);

            await UpdateOnlinePlayers();

            await Task.Delay(200);

            //await UpdateStatusChannelAsync();
        }

        private const ulong _statusChannelId = 1111211859283021884;
        public async Task UpdateStatusChannelAsync()
        {
            var guild = _discord.GetGuild(GuildService._officialGuildId);

            var channel = guild.GetTextChannel(_statusChannelId);

            var lastMessage = await channel.GetMessagesAsync(1).FlattenAsync();
            if (lastMessage is not null && lastMessage.Any()) await lastMessage.FirstOrDefault().DeleteAsync();

            var servers = _dynastioClient.OnlineServers
                .OrderByDescending(a => a.IsPrivate)
                .ThenBy(a => a.TopPlayerScore)
                .ToList();

            var content = servers
                .ToStringTable(new[] { "#", "server", "count", "limit", "private", "region", "top score" },
                        a => servers.IndexOf(a),
                        a => a.Label.TrySubstring(16),
                        a => a.PlayersCount.Metric(),
                        a => a.ConnectionsLimit.Metric(),
                        a => a.IsPrivate,
                        a => a.Region,
                        a => a.TopPlayerScore.Metric())
                        .ToMarkdown();

            var msg = await channel.SendMessageAsync(
                text: "",
                embed: new EmbedBuilder()
                {
                    Description = content
                }
                .Build());

            await msg.CrosspostAsync();
        }

        private const ulong _onlinePlayersChannelId = 1109014316922978354;
        public async Task UpdateOnlineServers()
        {
            var guild = _discord.GetGuild(GuildService._officialGuildId);

            var serverchannel = guild.GetTextChannel(_onlineServersChannelId);
            int serversCount = _dynastioClient.OnlineServers.Where(a => a.IsPrivate == false).Count();
            if (getNumber(serverchannel.Name) != serversCount)
                await serverchannel.ModifyAsync(
                    x =>
                    x.Name = $"Online Servers: {serversCount}");
        }

        private const ulong _onlineServersChannelId = 1109019161587355738;
        public async Task UpdateOnlinePlayers()
        {
            var guild = _discord.GetGuild(GuildService._officialGuildId);

            var playerschannel = guild.GetTextChannel(_onlinePlayersChannelId);
            int playerscount = _dynastioClient.OnlinePlayers.Count;
            if (getNumber(playerschannel.Name) != playerscount)
                await playerschannel.ModifyAsync(
                x =>
                x.Name = $"Online Players: {playerscount}");
        }

        private static int getNumber(string txt)
        {
            var resultString = Regex.Match(txt, @"\d+").Value;
            bool result = int.TryParse(resultString, out var number);
            return result ? number : 0;
        }
    }
}
