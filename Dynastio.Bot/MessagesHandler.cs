using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Dynastio.Bot
{
    /// <summary>
    /// Handles incoming Discord messages, routes AI mentions,
    /// and awards XP via the ranking system.
    /// </summary>
    internal sealed class MessagesHandler : IDisposable
    {
        private readonly UsersService _users;
        private readonly DynastioApi _dynastio;
        private readonly RankingService _ranker;
        private readonly ClientService _clientService;
        private readonly DiscordSocketClient _discord;
        private readonly DynastioBotDatabase _database;

        private readonly CommandHandlerService _commandHandlerService;

        public MessagesHandler(IServiceProvider services)
        {
            _users = services.GetRequiredService<UsersService>();
            _dynastio = services.GetRequiredService<DynastioApi>();
            _ranker = services.GetRequiredService<RankingService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _clientService = services.GetRequiredService<ClientService>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _commandHandlerService = services.GetRequiredService<CommandHandlerService>();

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
        {
            // 1) Filter out non-user messages, bots, and non-text channels
            if (rawMessage is not SocketUserMessage msg || msg.Source != MessageSource.User || msg.Author.IsBot || msg.Channel is not ITextChannel textChannel)
                return;

            try
            {
                // 2) Load guild and user profile
                var guild = await _database.GetGuildAsync(textChannel.GuildId, true);
                var user = await _users.GetOrCreateUserAsync(msg.Author.Id);

                var result = await _commandHandlerService.MessageReceivedAsync(msg);

                if (result is not null)
                    if (!result.IsSuccess)
                        if (!_clientService.HasTeamRole(user.Id))
                        {
                            await msg.AddReactionAsync(Emoji.Parse(":joy:"));
                            return;
                        }

                if (result == null || !result.IsSuccess)
                    await _ranker.TryAddMessageXpAsync(guild, user, msg);

            }
            catch (Exception ex)
            {

            }
        }



        public void Dispose()
        {
            _discord.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}
