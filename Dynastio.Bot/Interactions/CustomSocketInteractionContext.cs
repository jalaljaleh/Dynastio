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
    using Dynastio.Data;
    using Dynastio.Bot.Globalization;
    using Microsoft.Extensions.DependencyInjection;

    public class CustomSocketInteractionContext : SocketInteractionContext
    {
        private readonly DynastioData _dynastioData;
        private readonly GuildService _guildService;
        private readonly GlobalizationService _globalizationService;
        private readonly IServiceProvider _services;
        public CustomSocketInteractionContext(DiscordSocketClient client, SocketInteraction interaction, IServiceProvider services, User user = null, Guild guild = null) : base(client, interaction)
        {
            _services = services;
            _dynastioData = _services.GetRequiredService<DynastioData>();
            _guildService = _services.GetRequiredService<GuildService>();
            _globalizationService = _services.GetRequiredService<GlobalizationService>();

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
                    _user = _dynastioData.GetUserAsync(this.User.Id, true).Result;
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
                    _guild = _dynastioData.GetGuildAsync(this.Guild.Id).Result;
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
                    _guildLocale = _globalizationService.GetOrDefault(this.Guild.PreferredLocale);
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
                    _userLocale = _globalizationService.GetOrDefault((this.Interaction.UserLocale));
                }
                return _userLocale;
            }
        }


    }
}
