using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;

namespace Dynastio.Bot.Events
{
    internal class ready_event : HandlersBase
    {
        private readonly RepeaterService _repeaterService;
        public ready_event(IServiceProvider services) : base(services)
        {
            _discord.Ready += _discord_Ready;
            _repeaterService = services.GetRequiredService<RepeaterService>();
        }

        private async Task _discord_Ready()
        {
            _repeaterService.AddAction(SetBotStatus, TimeSpan.FromMinutes(10));
            
        }
        public async Task SetBotStatus()
        {
            await _discord.SetStatusAsync(UserStatus.Idle);

            await _discord.SetGameAsync(_discord.Guilds.Count + " servers " + _discord.Guilds.Select(a => a.MemberCount).Sum().Metric() + " Members", "https://www.youtube.com/watch?v=v74AQTvjtSg", ActivityType.Streaming);

        }

    }
}
