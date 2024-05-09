/*!
 * Discord Template By (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Project Url (https://github.com/jalaljaleh/Template.Discord.Bot/)
 */
namespace Dynastio.Bot.Interactions
{
    using Discord;
    using Discord.Interactions;
    using Discord.WebSocket;
    using Dynastio.Bot.Database;
    using Dynastio.Bot.Globalization;
    using Dynastio.Bot.Services;
    using Microsoft.Extensions.DependencyInjection;

    public class BotSocketInteractionContext : SocketInteractionContext
    {
        public readonly DynastioBotDatabase _db;
        public readonly AdvertisingService _ads;
        public readonly UserService _usersService;
        private readonly DynastioBotGlobalization _globalization;
        public readonly IServiceProvider _services;
        public BotSocketInteractionContext(DiscordSocketClient client, SocketInteraction interaction, IServiceProvider services, User user = null, Guild guild = null) : base(client, interaction)
        {
            _services = services;
            _ads = _services.GetRequiredService<AdvertisingService>();
            _db = _services.GetRequiredService<DynastioBotDatabase>();
            _globalization = _services.GetRequiredService<DynastioBotGlobalization>();
            _usersService = _services.GetRequiredService<UserService>();

            if (user != null) _user = user;
            if (guild != null) _guild = guild;
        }
        public SocketInteraction OverridedInteraction { get; set; }
        public object CustomData { get; set; }

        private User _user;
        private Guild _guild;
        private Locale _userLocale;
        private Locale _guildLocale;

        public User BotUser
        {
            get
            {
                if (_user is null)
                {
                    _user = _db.GetUserAsync(this.User.Id, true).Result;
                }
                return _user;
            }
        }
        public Guild BotGuild
        {
            get
            {
                if (_guild is null)
                {
                    _guild = _db.GetGuildAsync(this.Guild.Id).Result;
                }
                return _guild;
            }
        }
        public Locale GuildLocale
        {
            get
            {
                if (_guildLocale is null)
                {
                    _guildLocale = _globalization.GetOrDefault(this.Guild.PreferredLocale);
                }
                return _guildLocale;
            }
        }
        public Locale UserLocale
        {
            get
            {
                if (_userLocale is null)
                {
                    _userLocale = _globalization.GetOrDefault(this.Interaction.UserLocale);
                }
                return _userLocale;
            }
        }


    }
}
