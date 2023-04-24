using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Dynastio.Data;
namespace Dynastio.Bot
{
    public class EventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly GuildService _guildService;
        private readonly LocaleService _localeService;
        private readonly IDatabaseContext _db;
        public EventHandler(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _configuration = services.GetRequiredService<Configuration>();
            _guildService = services.GetRequiredService<GuildService>();
            _localeService = services.GetRequiredService<LocaleService>();
            _db = services.GetRequiredService<IDatabaseContext>();
            _services = services;
        }
        public void Initialize()
        {
            _client.Ready += _client_Ready;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.LeftGuild += _client_LeftGuild;
            _client.UserJoined += _client_UserJoined;
        }

        private async Task _client_Ready()
        {
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync(_configuration.BotStatus, "", ActivityType.Playing);

            if (!Program.IsDebug())
            {
                await LeaveExtraGuild().Try();
                ChannelUtilities.CheckImageOnlyChannels(_db, _client, _localeService).RunInBackground(true);
            }
        }

        private async Task _client_LeftGuild(SocketGuild guild)
        {
            if (_configuration.Guilds.MainServer == 0 || _configuration.Channels.JoinLeftLoggerChannel == 0)
                return;

            await _client.GetGuild(_configuration.Guilds.MainServer)
                .GetTextChannel(_configuration.Channels.JoinLeftLoggerChannel)
                .SendMessageAsync($"**-** Left `{guild.Name}` with `{guild.MemberCount}` members.")
                .Try();
        }
        private async Task _client_JoinedGuild(SocketGuild guild)
        {
            if (_configuration.Guilds.MainServer == 0 || _configuration.Channels.JoinLeftLoggerChannel == 0)
                return;

            await _client.GetGuild(_configuration.Guilds.MainServer)
                .GetTextChannel(_configuration.Channels.JoinLeftLoggerChannel)
                .SendMessageAsync($"**+** Joined `{guild.Name}` with `{guild.MemberCount}` members, `<@{guild.OwnerId}>` - Guilds **`{_client.Guilds.Count}`**")
                .Try();

            await LeaveExtraGuild().Try();
        }
        public async Task LeaveExtraGuild(string reason = null)
        {
            if (_client.Guilds.Count <= 99)
                return;

            var gLeave = _client.Guilds.OrderBy(a => a.MemberCount).Where(a => true).FirstOrDefault();
            await gLeave.Owner.SendMessageAsync($"we are supporting 100 servers only, i invited to new server with more users, i have to leave from your server {gLeave.Name}.\n" +
                $"The robot acts like an auction, whichever server has more members, the robot prefers that server.\n" +
                $"You need more members\nSupport & More {_configuration.Guilds.MainGuildInviteLink}").Try();
            await gLeave.LeaveAsync();
        }
        private async Task _client_UserJoined(SocketGuildUser arg)
        {
            await arg.SendMessageAsync(
                text: "https://www.youtube.com/watch?v=x1tRXvcFJvs" + "\n" + _configuration.Guilds.MainGuildInviteLink,
                components: new ComponentBuilder()
                        .WithButton("Как взломать монеты ?", null, ButtonStyle.Link, null, _configuration.DynastioYoutubeChannelId.ToYoutubeChannelSubConfirmation())
                        .WithButton("How To Hack Dynast.io ?", null, ButtonStyle.Link, null, _configuration.DynastioYoutubeChannelId.ToYoutubeChannelSubConfirmation())
                        .WithButton("Official Dynast.io Channel", null, ButtonStyle.Link, null, _configuration.DynastioYoutubeChannelId.ToYoutubeChannelSubConfirmation())
            .Build()).Try();

        }
    }
}
