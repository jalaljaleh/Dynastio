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
    using Dynastio.Bot.Addons;
    using Dynastio.Bot.Database;
    using Dynastio.Bot.Extenstions;
    using Dynastio.Bot.Globalization;
    using Dynastio.Bot.Helpers;
    using Dynastio.Bot.Interactions.Modules.shared_buttons;
    using Dynastio.Bot.Services;
    using Microsoft.VisualBasic;
    using System.ComponentModel;

    public class BotInteractionModuleBase : InteractionModuleBase<BotSocketInteractionContext>
    {
        public string this[string key] { get => Context.UserLocale[key]; }
        public string this[string key, params object[] @params] { get => Context.UserLocale[key, @params]; }
        public BotInteractionModuleBase() : base()
        {

        }
        public AdvertisingService advertisingService { get => Context._ads; }
        public DynastioBotDatabase dynastioBotDatabase { get => Context._dynastioData; }
        public IServiceProvider services { get => Context._services; }

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
        public async Task<bool> UpdateBotGuildAsync()
        {
            return await this.dynastioBotDatabase.UpdateAsync(this.BotGuild);
        }
        public async Task<bool> UpdateBotUserAsync()
        {
            return await this.dynastioBotDatabase.UpdateAsync(this.BotUser);
        }
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
        public async Task<IUserMessage> ClearCurrentMessageComponentsAsync()
        {
            await CurrentMessage.ModifyAsync(x =>
            {
                x.Components = new ComponentBuilder().Build();
            });
            return CurrentMessage;
        }
        public async Task CloseMenuAsync()
        {
            await ModifyCurrentMessageAsync(
             embed: (userLocale["menu_closed_description"] + "\n\n" + advertisingService.GetInlineEmbedDescription())
                    .ToEmbed(userLocale["menu_closed_title"],
                    Context.Client.CurrentUser.TryGetAvatarUrl()
                    ));
        }
        public async Task<bool> ConfirmActionAsync()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Confirmation",
                Description = "Are you sure about executing the command, The action may not be undone.",
                Color = Color.Orange
            }.Build();

            var component = new ComponentBuilder()
                .WithButton("Confirm", DiscordInput.GenerateCustomId("confirm"), ButtonStyle.Success)
                .WithButton("Cancel", DiscordInput.GenerateCustomId("cancel"), ButtonStyle.Danger)
                .Build();

            var message = await ModifyCurrentMessageAsync(embed: embed, components: component);
            var interactionResult = await Context.WaitForButtonFromMessageAsync(message, TimeSpan.FromSeconds(60), true, true, true);

            if (interactionResult == null || interactionResult.Data.CustomId != DiscordInput.GetCustomId("confirm"))
                return false;

            return true;
        }

        public async Task<UserAccount> SelectUserAccountAsync()
        {

            // Combine Defer and SelectMenu creation for efficiency
            var selectMenu = new SelectMenuBuilder(
                DiscordInput.GenerateCustomId("user.accounts"),
                null,
                Context.UserLocale["menu.profile.accounts.choose"],
                1,
                1,
                false,
                ComponentType.SelectMenu
            );

            // Use LINQ to efficiently filter and map accounts
            var options = BotUser.Accounts
                .Take(20)
                .Select(acc => new SelectMenuOptionBuilder(
                    acc.Reminder,
                    acc.GetHashCode().ToString(),
                    acc.GetAccountService(),
                    null,
                    false
                ))
                .ToList();

            selectMenu.WithOptions(options);

            var components = new ComponentBuilder()
                .WithSelectMenu(selectMenu)
                .WithButton(CancelButton.GetButton(userLocale), 1)
                .Build();

            var embed = new EmbedBuilder()
            {
                Title = userLocale["account_selection"],
                Description =
                userLocale["account_selection_description"] + "\n**" +
                userLocale["undo_action_description"] + "**\n" +
                userLocale["menu_closes", DateTime.UtcNow.AddSeconds(30).UnixTimestampDiscordFormat()],
                ThumbnailUrl = Context.Client.CurrentUser.TryGetAvatarUrl(),
                Color = EmbedsHelper.ColorWaitingResopnse,
            };
            var message = await ModifyCurrentMessageAsync(Context.User.Mention, components: components, embed: embed.Build());

            var result = await Context.WaitForSelectMenuFromMessageAsync(message, TimeSpan.FromSeconds(30));
            if (result is null || BotUser.GetAccountByHashCode(result.Data.Values.FirstOrDefault(), out UserAccount account) is null)
            {
                return null;
            }
            return account;
        }
    }

}
