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

        public async Task<IUserMessage> FollowUpToLoading(string Description = null, string Title = null, string ThumbnailUrl = null)
        {
            return await FollowupAsync(userMention, embed: new EmbedBuilder()
            {
                Title = Title ?? this["waiting_for_bot.title"],
                Description = Description ?? this["waiting_for_bot.description"] + "\n" +
                                       $"\n\n" + this["waiting_for_bot.waiting_since:*", DateTime.UtcNow.ToDiscordUnixTimestampFormat()].ToBold(),
                ThumbnailUrl = ThumbnailUrl ?? "https://cdn.discordapp.com/attachments/1098332386674085988/1106640674234847262/1496_1.gif",
                Color = Color.LightOrange
            }.Build()); ;
        }

    }

}
