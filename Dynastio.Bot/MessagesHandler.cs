using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dynastio.Bot
{
    /// <summary>
    /// Handles incoming Discord messages and awards XP via the ranking system.
    /// </summary>
    internal sealed class MessagesHandler : IDisposable
    {
        private readonly DiscordSocketClient _discord;
        private readonly DynastioBotDatabase _database;
        private readonly ConfigurationService _config;
        private readonly XpRankingSystemService _xpRanking;
        private readonly UsersService _users;

        /// <summary>
        /// Constructs a new instance, resolving required services.
        /// </summary>
        public MessagesHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<ConfigurationService>();
            _xpRanking = services.GetRequiredService<XpRankingSystemService>();
            _users = services.GetRequiredService<UsersService>();

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        /// <summary>
        /// Fired whenever any message is received.  
        /// Filters out non-user or non-text‐channel messages, then awards XP.
        /// </summary>
        private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Source != MessageSource.User || rawMessage.Author.IsBot || rawMessage is not SocketUserMessage userMessage)
                return;


            if (userMessage.Channel is not ITextChannel textChannel)
                return;

            try
            {
                var guild = await _database.GetGuildAsync(textChannel.GuildId, true, () => GuildFactory.CreateDefault(textChannel.GuildId), default);

                var user = await _users.GetUserAsync(userMessage.Author.Id, true);

                await _xpRanking.TryAddMessageXpAsync(guild, user, userMessage);
            }
            catch (Exception ex)
            {
                Common.Log("Messages Handler", $"Failed to process message from UserId={userMessage.Author.Id} in GuildId={textChannel.GuildId}");
            }
        }

        /// <summary>
        /// Unhooks events when disposing.
        /// </summary>
        public void Dispose()
        {
            _discord.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}