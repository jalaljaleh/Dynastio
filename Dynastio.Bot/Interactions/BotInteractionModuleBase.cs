/*!
 * Discord Template By (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Project Url (https://github.com/jalaljaleh/Template.Discord.Bot/)
 */
namespace Dynastio.Bot.Interactions
{
    using Amazon.SecurityToken.Model;
    using Discord;
    using Discord.Interactions;
    using Discord.WebSocket;
    using Dynastio.Bot.Database;
    using Dynastio.Bot.Extenstions;
    using Dynastio.Bot.Globalization;
    using Dynastio.Bot.Services;
    using Microsoft.VisualBasic;

    public class BotInteractionModuleBase : InteractionModuleBase<BotSocketInteractionContext>
    {
        public string this[string key] { get => Context.UserLocale[key]; }
        public string this[string key, params object[] @params] { get => Context.UserLocale[key, @params]; }
        public BotInteractionModuleBase() : base()
        {

        }
        public AdvertisingService advertisingService { get => Context._ads; }
        public DynastioBotDatabase dynastioBotDatabase { get => Context._dynastioData; }
        public User BotUser { get => Context.BotUser; }
        public Guild BotGuild { get => Context.BotGuild; }
        public Locale guildLocale { get => this.Context.GuildLocale; }
        public Locale userLocale { get => this.Context.UserLocale; }
        public string userMention => Context.User.Mention;


        public string BotAvatarUrl { get => this.Context.Client.CurrentUser.TryGetAvatarUrl(); }

        public IUserMessage CurrentMessage =>
             Context.Interaction.Type switch
             {
                 InteractionType.ModalSubmit => (Context.Interaction as SocketModal).Message,
                 InteractionType.MessageComponent => (Context.Interaction as SocketMessageComponent).Message,
                 _ => null
             };

        public async Task<IUserMessage> ModifyCurrentMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
        {
            await CurrentMessage.ModifyAsync(x =>
             {
                 x.Content = text;
                 x.AllowedMentions = allowedMentions;
                 x.Attachments = null;
                 x.Components = components;
                 x.Embed = embed;
                 x.Embeds = embeds;
                 x.Flags = null;
             });
            return CurrentMessage;
        }
        public async Task ModifyCurrentMessageToInputModeAsync()
        {
            await ModifyCurrentMessageAsync(
               Context.User.Mention,
               embed: userLocale["input_mode.description"].ToEmbed(userLocale["input_mode.title"], thumbnailUrl: Context.Client.CurrentUser.TryGetAvatarUrl())
               );
        }
        public async Task<bool> DeleteCurrentMessageAsync()
        {
            return await CurrentMessage.DeleteAsync().TryAsync();
        }
    }

}
