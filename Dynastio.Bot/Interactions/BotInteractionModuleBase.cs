using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally.Extensions;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions
{
    //
    // Summary:
    //     Provides a base class for a command module to inherit from.
    //
    // Type parameters:
    //   T:
    //     Type of interaction context to be injected into the module.
    public class BotInteractionModuleBase : BotInteractionModuleBase<BotSocketInteractionContext>;
    public class BotInteractionModuleBase<T> : InteractionModuleBase<T> where T : BotSocketInteractionContext
    {

        public UsersService UsersService { get => Context.UsersService; }
        public GuildServices GuildService { get => Context.GuildService; }

        public EmoteService EmoteService { get => Context.EmoteService; }
        public AssetUrlService AssetUrlService { get => Context.AssetUrlService; }



        /// <summary>
        /// Access localized strings using a key.
        /// </summary>
        public string this[string key] => I18nExtensions.Tfin(key, this.Context.Interaction.UserLocale, null);

        /// <summary>
        /// Access localized strings with formatting parameters.
        /// </summary>
        public string this[string key, object model] => I18nExtensions.Tfin(key, Context.Interaction.UserLocale, model);



        /// <summary>
        /// Gets the bot's internal user model.
        /// </summary>
        public User BotUser => Context.BotUser;

        /// <summary>
        /// Gets the bot's internal guild model.
        /// </summary>
        public Guild BotGuild => Context.BotGuild;

        // ----------------     Discord     ---------------------------------------------------------


        /// <summary>
        /// Gets the Discord's Socket User model.
        /// </summary>
        public SocketUser User => Context.User;
        /// <summary>
        /// Gets the Discord's Socket Guild model.
        /// </summary>
        public SocketGuild Guild => Context.Guild;



        /// <summary>
        /// Gets the Discord mention string for the current user.
        /// </summary>
        public string UserMention => Context.User.Mention;

        /// <summary>
        /// Gets the bot's avatar URL.
        /// </summary>
        public string BotAvatarUrl => Context.Client.CurrentUser.TryGetAvatarUrl();



        /// <summary>
        /// Gets the message associated with the current interaction, if applicable.
        /// </summary>
        public IUserMessage CurrentMessage =>
            Context.Interaction.Type switch
            {
                InteractionType.ModalSubmit => (Context.Interaction as SocketModal)?.Message,
                InteractionType.MessageComponent => (Context.Interaction as SocketMessageComponent)?.Message,
                //     InteractionType.ApplicationCommand => (Context.Interaction as Component)
                _ => null
            };

        /// <summary>
        /// Updates the bot's user profile in the database.
        /// </summary>
        public async Task<bool> UpdateBotUserAsync()
        {
            return await Context.UsersService.UpdateUserAsync(BotUser);
        }

        /// <summary>
        /// Placeholder for updating the bot's guild profile.
        /// </summary>
        public async Task<bool> UpdateBotGuildAsync()
        {
            return await Context.GuildService.UpdateGuildAsync(BotGuild);
        }

        /// <summary>
        /// Defer Current Interaction 
        /// </summary>
        protected override Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {
            base.Context.IsDeferred = true;
            return Context.Interaction.DeferAsync(ephemeral, options);
        }



        // 🔒 All other methods are commented for now.

        //public async Task ModifyCurrentMessageToInputModeAsync()
        //{
        //    var embed = new EmbedBuilder()
        //    {
        //        Title = userLocale["input_mode.title"],
        //        Description = userLocale["input_mode.description"],
        //        ThumbnailUrl = BotAvatarUrl,
        //        Color = Color.Orange,
        //        Fields = new List<EmbedFieldBuilder>()
        //        {
        //            new EmbedFieldBuilder().WithIsInline(true)
        //            .WithName("Waiting time")
        //            .WithValue("Since " +DateTime.UtcNow.UnixTimestampDiscordFormat())
        //        },
        //    }.Build();
        //    await ModifyCurrentMessageAsync(userMention, embed: embed,components: new ComponentBuilder().Build());
        //}
        //public async Task<bool> DeleteCurrentMessageAsync()
        //{
        //    return await CurrentMessage.DeleteAsync().TryAsync();
        //}
        //public async Task<IUserMessage> ClearCurrentMessageComponentsAsync()
        //{
        //    await CurrentMessage.ModifyAsync(x =>
        //    {
        //        x.Components = new ComponentBuilder().Build();
        //    });
        //    return CurrentMessage;
        //}
        //public async Task CloseMenuAsync()
        //{
        //    var embed = new EmbedBuilder()
        //    {
        //        Title = userLocale["menu_closed_title"],
        //        Description = userLocale["menu_closed_description"] + "\n\n",
        //        ThumbnailUrl = BotAvatarUrl,
        //        Color = Color.Orange,
        //        Fields = new List<EmbedFieldBuilder>()
        //        {
        //            new EmbedFieldBuilder().WithIsInline(true)
        //            .WithName("Waiting time")
        //            .WithValue("Since " +DateTime.UtcNow.UnixTimestampDiscordFormat())
        //        },
        //    }.Build();

        //    await ModifyCurrentMessageAsync(userMention, embed: embed, components: new ComponentBuilder().Build());
        //}
        //public async Task<bool> ConfirmActionAsync()
        //{
        //    var embed = new EmbedBuilder()
        //    {
        //        Title = "Confirmation",
        //        Description = "Are you sure about executing the command, The action may not be undone.",
        //        Color = Color.Orange
        //    }.Build();

        //    var component = new ComponentBuilder()
        //        .WithButton("Confirm", DiscordInput.GenerateCustomId("confirm"), ButtonStyle.Success)
        //        .WithButton("Cancel", DiscordInput.GenerateCustomId("cancel"), ButtonStyle.Danger)
        //        .Build();

        //    var message = await ModifyCurrentMessageAsync(embed: embed, components: component);
        //    var interactionResult = await Context.WaitForButtonFromMessageAsync(message, TimeSpan.FromSeconds(60), true, true, true);

        //    if (interactionResult == null || interactionResult.Data.CustomId != DiscordInput.GetCustomId("confirm"))
        //        return false;

        //    return true;
        //}

        //public async Task<UserGameAccount> SelectUserAccountAsync()
        //{

        //    // Combine Defer and SelectMenu creation for efficiency
        //    var selectMenu = new SelectMenuBuilder(
        //        DiscordInput.GenerateCustomId("user.accounts"),
        //        null,
        //        Context.UserLocale["menu.profile.accounts.choose"],
        //        1,
        //        1,
        //        false,
        //        ComponentType.SelectMenu
        //    );

        //    // Use LINQ to efficiently filter and map accounts
        //    var options = BotUser.Accounts
        //        .Take(20)
        //        .Select(acc => new SelectMenuOptionBuilder(
        //            acc.Reminder,
        //            acc.GetHashCode().ToString(),
        //            acc.GetAccountService(),
        //            null,
        //            false
        //        ))
        //        .ToList();

        //    selectMenu.WithOptions(options);

        //    var components = new ComponentBuilder()
        //        .WithSelectMenu(selectMenu)
        //       // .WithButton(CancelButton.GetButton(userLocale), 1)
        //        .Build();

        //    var embed = new EmbedBuilder()
        //    {
        //        Title = userLocale["account_selection"],
        //        Description =
        //        userLocale["account_selection_description"] + "\n**" +
        //        userLocale["undo_action_description"] + "**\n" +
        //        userLocale["menu_closes", DateTime.UtcNow.AddSeconds(30).UnixTimestampDiscordFormat()],
        //        ThumbnailUrl = Context.Client.CurrentUser.TryGetAvatarUrl(),
        //    };
        //    var message = await ModifyCurrentMessageAsync(Context.User.Mention, components: components, embed: embed.Build());

        //    var result = await Context.WaitForSelectMenuFromMessageAsync(message, TimeSpan.FromSeconds(30));
        //    if (result is null || BotUser.GetAccountByHashCode(result.Data.Values.FirstOrDefault(), out UserGameAccount account) is null)
        //    {
        //        return null;
        //    }
        //    return account;
        //}
    }
}