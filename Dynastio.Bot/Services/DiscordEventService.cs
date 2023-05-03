using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    internal partial class DiscordEventService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;

        public DiscordEventService(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _configuration = _services.GetRequiredService<Configuration>();

            _client.Ready += _client_Ready;
        }
        private async Task _client_Ready()
        {

            var guilds = _client.Guilds.ToList();

            await _client.SetGameAsync($"Watching {guilds.Sum(a => a.MemberCount)} members !", "", ActivityType.Playing);
            await _client.SetStatusAsync(UserStatus.Online);

        }
    }
}
