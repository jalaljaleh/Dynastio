using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot.Interactions
{
    /// <summary>
    /// Extended interaction context that provides access to bot-specific services and data.
    /// </summary>
    public class BotSocketInteractionContext : SocketInteractionContext
    {
        public readonly IServiceProvider _services;

        public readonly InteractionService InteractionService;
        public readonly UsersService UsersService;
        public readonly GuildServices GuildService;

        public readonly EmoteService EmoteService;
        public readonly AssetUrlService AssetUrlService;
        public readonly DynastioBotDatabase Database;

        public readonly DynastioApi Dynastio;

        private User _user;
        private Guild _guild;

        public BotSocketInteractionContext(DiscordSocketClient client,SocketInteraction interaction,IServiceProvider services,User user = null,Guild guild = null): base(client, interaction)
        {
            _services = services;
            UsersService = services.GetRequiredService<UsersService>();
            GuildService = services.GetRequiredService<GuildServices>();
            EmoteService = services.GetRequiredService<EmoteService>();
            AssetUrlService = services.GetRequiredService<AssetUrlService>();
            Database = services.GetRequiredService<DynastioBotDatabase>();
            Dynastio = services.GetRequiredService<DynastioApi>();

            InteractionService = services.GetRequiredService<InteractionService>();

            _user = user;
            _guild = guild;
        }

        public bool IsDeferred = false;

        /// <summary>
        /// Gets the bot's internal user model for the current interaction user.
        /// </summary>
        public User BotUser => _user ??= Database.GetUserAsync(User.Id, true).Result;

        /// <summary>
        /// Gets the bot's internal guild model for the current interaction guild.
        /// </summary>
        public Guild BotGuild => _guild ??= Database.GetGuildAsync(Guild.Id).Result;

    }
}