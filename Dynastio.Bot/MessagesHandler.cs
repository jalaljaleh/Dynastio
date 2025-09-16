using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Global.Helper;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// Central message router:
    /// 1. Filters out irrelevant messages (bots, DMs, non‐text channels).  
    /// 2. Dispatches commands.  
    /// 3. Invokes AI replies for developer mentions.  
    /// 4. Awards XP for ordinary messages.  
    /// </summary>
    internal sealed class MessagesHandler : IDisposable
    {
        private const ulong DevUserId = 1374305522290917526UL;

        private readonly DiscordSocketClient _discord;
        private readonly DynastioBotDatabase _database;
        private readonly UsersService _users;
        private readonly RankingService _ranker;
        private readonly ClientService _clientService;
        private readonly CommandHandlerService _commandHandler;
        private readonly CommandService _commands;
        private readonly AiChatService _aiChat;
        private readonly DynastioApi _dynastioApi;
        private readonly IServiceProvider _services;

        public MessagesHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _users = services.GetRequiredService<UsersService>();
            _ranker = services.GetRequiredService<RankingService>();
            _clientService = services.GetRequiredService<ClientService>();
            _commandHandler = services.GetRequiredService<CommandHandlerService>();
            _aiChat = services.GetRequiredService<AiChatService>();
            _dynastioApi = services.GetRequiredService<DynastioApi>();
            _commands = services.GetRequiredService<CommandService>();
            _services = services;
            // Subscribe to all incoming messages
            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        /// <summary>
        /// Entry point for every Discord message. Filters, routes commands or AI, and awards XP.
        /// </summary>
        private async Task OnMessageReceivedAsync(SocketMessage raw)
        {
            // 1. Ignore system/bot messages, DMs, and non-text channels
            if (raw is not SocketUserMessage message || message.Source != MessageSource.User || message.Author.IsBot || message.Channel is not ITextChannel textChannel)
                return;


            try
            {
                var guild = await _database.GetGuildAsync(textChannel.GuildId).ConfigureAwait(false);
                var user = await _users.GetOrCreateUserAsync(message.Author.Id).ConfigureAwait(false);

                var cmdResult = await _commandHandler.MessageReceivedAsync(message).ConfigureAwait(false);
                if (cmdResult is null)
                {
                    await _ranker.TryAddMessageXpAsync(guild, user, message).ConfigureAwait(false);
                    return;
                }

                if (guild.Id != 480416088312774657) return;

                // 4. If command handler returned a result, handle AI or reaction
                if (!cmdResult.IsSuccess)
                {
                    if (!_aiChat.TryAcquireSlot())
                    {
                        await message.AddReactionAsync(new Emoji("😂")).ConfigureAwait(false);
                        return;

                    }
                    await _aiChat.ReplyMessageAsync(message, user).ConfigureAwait(false);
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing incoming message");
            }
        }

        /// <summary>
        /// Unsubscribe from events to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _discord.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}
