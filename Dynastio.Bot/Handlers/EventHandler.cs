using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Managers;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal partial class EventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly GuildService _guildService;

        public EventHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _configuration = _services.GetRequiredService<Configuration>();
            _guildService = _services.GetService<GuildService>();
            _client.Ready += _client_Ready;
            _client.GuildMemberUpdated += _client_GuildMemberUpdated;
        }

        private async Task _client_GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> olduser, SocketGuildUser newUser)
        {
            //user boosted the server
            if (olduser.Value.PremiumSince.HasValue is false && newUser.PremiumSince.HasValue)
            {
                //  _guildService.GetChannelId(GuildChannelType.)
            }
        }


        private async Task _client_Ready()
        {
            await _services.GetRequiredService<WebhookService>().InitializeAsync();

            var guilds = _client.Guilds.ToList();

            await _client.SetGameAsync($"Watching {guilds.Sum(a => a.MemberCount)} members !", "", ActivityType.Playing);
            await _client.SetStatusAsync(UserStatus.Online);

            //foreach(var guild in guilds)
            //{
            //    if(guild.Id == 480416088312774657) continue;
            //    await guild.LeaveAsync();
            //}
 
        }
    }
}
