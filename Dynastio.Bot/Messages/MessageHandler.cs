using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
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
        private readonly UserService _userService;
        private readonly RankService _rankService;
        private readonly GuildService _guildService;
        private readonly WebhookService _webhookService;
        public MessageHandler(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = services.GetRequiredService<CommandsHandler>();
            _userService = services.GetRequiredService<UserService>();
            _rankService = services.GetRequiredService<RankService>();
            _guildService = services.GetRequiredService<GuildService>();
            _webhookService = services.GetRequiredService<WebhookService>();
            _discord.MessageReceived += _discord_MessageReceived;
            
            if (!Global.Main.IsDebug())
            { 
                _discord.MessageDeleted += _discord_MessageDeleted;
                _discord.MessageUpdated += _discord_MessageUpdated;
            }
         
        }

        private async Task _discord_MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage NewMessage, ISocketMessageChannel channel)
        {
            if (!(NewMessage is SocketUserMessage message))
                return;

            if (NewMessage.Source != Discord.MessageSource.User)
                return;

            if (channel is IGuildChannel guildChannel)
            {
                if (guildChannel.GuildId != GuildService._officialGuildId) return;

                var _oldMessage = await oldMessage.GetOrDownloadAsync();

                if (_oldMessage is null) return;

                await _webhookService.LogEditedMessageAsync(NewMessage,_oldMessage, channel);
            }
        }

        private async Task _discord_MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        {
           
            if (channel.HasValue && channel.Value is IGuildChannel guildChannel)
            {
                if (guildChannel.GuildId != GuildService._officialGuildId) return;

                var message = await cachedMessage.GetOrDownloadAsync();
               
                if (!(message is SocketUserMessage))
                    return;

                if (message.Source != Discord.MessageSource.User)
                    return;

                if (message is null) return;

                await _webhookService.LogDeleteMessageAsync(message, channel.Value);
            }
        }

        private async Task _discord_MessageReceived(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != Discord.MessageSource.User)
                return;

            try
            {
                var commandResult = await _commandHandler.HandleCommand(message);
            }
            catch
            {

            }

            await _rankService.AddMessageScoreAsync(message);
        }
    }
}
