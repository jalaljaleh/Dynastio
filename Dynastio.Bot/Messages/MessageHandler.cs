using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Dynastio.Bot
{
    public class MessageHandler
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        private readonly CommandsHandler _commandHandler;

        public MessageHandler(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = services.GetRequiredService<CommandsHandler>();

            _discord.MessageReceived += _discord_MessageReceived;
        }

        private async Task _discord_MessageReceived(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != Discord.MessageSource.User)
                return;

            var commandResult = await _commandHandler.HandleCommand(message);
            if (commandResult is null) return;

        }
    }
}
