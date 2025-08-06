using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Dynastio.Bot.Database;
using Dynastio.Bot.Entities;

namespace Dynastio.Bot.Handlers
{
    internal class MessagesHandler
    {
        private readonly XpRankingSystemService _xpRankingSystem;
        public readonly DiscordSocketClient _discord;
        public readonly DynastioBotDatabase _db;
        public readonly AppConfiguration _config;
        public MessagesHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<AppConfiguration>();
            _xpRankingSystem = services.GetRequiredService<XpRankingSystemService>();

            _discord.MessageReceived += _discord_MessageReceived;
        }

        private async Task _discord_MessageReceived(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            // var commandResult = await _commandHandler.HandleCommand(message);

            if (message.Channel is ITextChannel txtChannel)
            {
                var guild = await _db.GetGuildAsync(txtChannel.GuildId, true, () => GuildFactory.CreateDefault(txtChannel.GuildId), null);
                var user = await _db.GetUserAsync(message.Author.Id, true, () => UserFactory.CreateDefault(message.Author.Id), null);

                await _xpRankingSystem.TryAddMessageXpAsync(guild, user, message);
            }

        }
    }
}
