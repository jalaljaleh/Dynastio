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
    using Dynastio.Bot.Extenstions;
    using Dynastio.Bot.Globalization;

    public class BotInteractionModuleBase : InteractionModuleBase<BotSocketInteractionContext>
    {

        public BotInteractionModuleBase() : base()
        {

        }

        public User BotUser { get => Context.BotUser; }
        public Locale guildLocale { get => this.Context.GuildLocale; }
        public Locale userLocale { get => this.Context.UserLocale; }
        public string this[string key] { get => Context.UserLocale[key]; }
        public string this[string key, params object[] @params] { get => Context.UserLocale[key, @params]; }

        public async Task<IUserMessage> ModifyCurrentMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
        {
            await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync(x =>
             {
                 x.Content = text;
                 x.AllowedMentions = allowedMentions;
                 x.Attachments = null;
                 x.Components = components;
                 x.Embed = embed;
                 x.Embeds = embeds;
                 x.Flags = null;
             });
            return (Context.Interaction as SocketMessageComponent).Message;
        }
        public async Task<bool> DeleteCurrentMessageAsync()
        {
            return await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync().TryAsync();
        }
    }

}
