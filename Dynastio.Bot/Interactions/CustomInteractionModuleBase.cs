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
    using Dynastio.Bot.Globalization;

    public class CustomInteractionModuleBase : InteractionModuleBase<CustomSocketInteractionContext>
    {

        public CustomInteractionModuleBase() : base()
        {

        }
        public string userMention => Context.User.Id.ToUserMention();
        public Locale thisG { get => this.Context.GuildLocale; }
        public Locale thisU { get => this.Context.UserLocale; }
        public string this[string key] { get => Context.UserLocale[key]; }
        public string this[string key, params object[] @params] { get => Context.UserLocale[key, @params]; }

    }

}
