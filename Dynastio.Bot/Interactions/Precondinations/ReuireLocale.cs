/*!
 * Discord RequireLocale v1.0 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/Template/Interactions/Precondinations/ReuireLocale.cs)
 */

namespace Discord.Interactions
{
    using Dynastio.Bot.Interactions;
    public class RequireLocale : PreconditionAttribute
    {
        public enum LocaleType { GuildLocale, UserLocale }

        private readonly string[] locales;
        private readonly LocaleType type;
        public RequireLocale(LocaleType type, params string[] locale)
        {
            this.locales = locale;
        }
        public RequireLocale(string locale)
        {
            this.locales = new string[] { locale };
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context_, ICommandInfo commandInfo, IServiceProvider services)
        {
            var context = context_ as CustomSocketInteractionContext;

            if (locales.Contains(
                type == LocaleType.UserLocale
                ? context.Interaction.UserLocale
                : context.Interaction.GuildLocale))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("precondination.error.require_locale"));
        }
    }
}
