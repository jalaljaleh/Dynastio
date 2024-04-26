using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Entities;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot.Events
{
    public class guilds_join_user
    {
        private readonly DynastioBotDatabase _db;
        private readonly AppConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly AdvertisingService _advertising;
        private readonly IServiceProvider _services;
        public guilds_join_user(IServiceProvider services)
        {
            _services = services;
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<AppConfiguration>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _advertising = services.GetRequiredService<AdvertisingService>();

            _discord.UserJoined += _discord_UserJoined;
        }

        private async Task _discord_UserJoined(SocketGuildUser user)
        {
            await RepeaterService.RepeatActionAsync(async () =>
            {
                await user.SendMessageAsync(_advertising.GetDirectMessageContent(),
                       embed: _advertising.GetInlineEmbedDescription(3).ToEmbed(),
                       components: _advertising.ExploitationAdvertisingButtons(new ComponentBuilder(), 0, 3).Build());

            }, 2, TimeSpan.FromSeconds(5));

        }


    }
}
