using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot.Handlers
{
    internal class MessagesHandler : HandlersBase
    {
        private readonly RankingService _rankingService;
        public MessagesHandler(IServiceProvider services) : base(services)
        {
            _rankingService = services.GetRequiredService<RankingService>();

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
                var guild = await _db.GetGuildAsync(txtChannel.GuildId);
                var user = await _db.GetUserAsync(message.Author.Id);

                await _rankingService.TryAddMessageXpAsync(guild, user, message);
            }

        }
    }
}
