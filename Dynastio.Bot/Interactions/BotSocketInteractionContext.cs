using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot.Interactions
{
    /// <summary>
    /// Extended interaction context that provides access to bot-specific services and data.
    /// </summary>
    public class BotSocketInteractionContext : SocketInteractionContext
    {
        private readonly DynastioBotDatabase _db;
        private readonly IServiceProvider _services;

        public UsersService UsersService { get; }
        public SocketInteraction OverridedInteraction { get; set; }
        public object CustomData { get; set; }

        private User _user;
        private Guild _guild;

        public BotSocketInteractionContext(DiscordSocketClient client,SocketInteraction interaction,IServiceProvider services,User user = null,Guild guild = null): base(client, interaction)
        {
            _services = services;
            _db = _services.GetRequiredService<DynastioBotDatabase>();
            UsersService = _services.GetRequiredService<UsersService>();

            _user = user;
            _guild = guild;
        }

        /// <summary>
        /// Gets the bot's internal user model for the current interaction user.
        /// </summary>
        public User BotUser => _user ??= _db.GetUserAsync(User.Id, true).Result;

        /// <summary>
        /// Gets the bot's internal guild model for the current interaction guild.
        /// </summary>
        public Guild BotGuild => _guild ??= _db.GetGuildAsync(Guild.Id).Result;

    }
}