using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
using Dynastio.Net;


namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonLoginModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public BadgesService BadgesRoleSyncService { get; set; }
        public DynastioApi Dynastio { get; set; }
        public DynastioBotDatabase Database { get; set; }
        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.login";


        public const string InteractionModalId = "interactions.menu.modal.login";

        /// <summary>
        /// Suffix format appended after the base ID.
        /// {0} = page, {1} = page size, {2} = trigger context.
        /// Allows you to pass parameters through the button’s CustomId.
        /// </summary>
        public const string IdParameterFormat = ":{0}";

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Construct the ButtonBuilder that appears in the UI whenever this module is used.
        /// Copy-and-paste this method into your new module and adjust:
        /// - Label text
        /// - Emote key
        /// - Button style
        /// - CustomId construction
        /// </summary>
        /// <param name="args">
        /// Optional string parameters that will be embedded in the CustomId.
        /// Helps pass context (like page or filter) back to ExecuteAsync.
        /// </param>
        /// <returns>A fully configured ButtonBuilder instance.</returns>
        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] args)
        {
            var btn = new ButtonBuilder()
                  //      .WithLabel(module["buttons.interactions.menu.login.label"])
                  .WithLabel("Add Account")
                .WithEmote(module.EmoteService.GetEmoteByName("privatechest"))
                .WithStyle(ButtonStyle.Success)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));

            return btn;
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Custom ID Factory
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Produces the CustomId string for this module’s button.
        /// Call this in your BuildButton override to ensure Consistent ID format.
        /// </summary>
        /// <param name="page">Page number (default = 1).</param>
        /// <param name="take">Items per page (default = 10).</param>
        /// <param name="trigger">Context label (default = empty).</param>
        /// <returns>Fully formatted CustomId for use with ComponentInteraction.</returns>
        public static string BuildCustomId(string trigger = "")
        {
            // Concatenate base prefix + formatted parameters
            // .StarIfNullFormat ensures safe formatting even if trigger is null/empty
            return InteractionIdBase
                 + IdParameterFormat.StarIfNullFormat(trigger);
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Interaction Handler
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// This method is invoked when Discord receives a button click
        /// whose CustomId matches InteractionIdBase + IdParameterFormat.
        /// Copy-and-paste into your new module and adjust the attribute:
        /// [ComponentInteraction(YourBase + YourFormat)]
        /// </summary>
        [ComponentInteraction(InteractionIdBase + ":*")]
        [RequireContext(ContextType.Guild)]
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        public async Task ExecuteAsync(string trigger = "")
        {
            if (Context.BotUser.HasLinkedAccount is false)
            {
                await RespondWithModalAsync<AccountLoginModal>(InteractionModalId);
                return;
            }

            if (BotUser.GetDefaultAccount() is null)
            {
                BotUser.SetDefaultAccount(BotUser.Accounts.FirstOrDefault().Id);
                await ReplyWithSuccessAsync($"🛠️ **{BotUser.GetDefaultAccount().DisplayName} ** Default account updated successfully ! Your Dynasty journey begins now.");
                return;
            }

            if (BotUser.Accounts.Count > 24)
            {
                await ReplyWithErrorAsync("You’ve reached your account limit. One warrior, one destiny.");
                return;
            }
            await RespondWithModalAsync<AccountLoginModal>(InteractionModalId);
        }

        public static string BypassPinCode = "";
        /// <summary>
        /// Receive modal submissions from SearchPlayerModalForm.
        /// Forwards values to main ExecuteAsync.
        /// </summary>
        [ModalInteraction(InteractionModalId)]
        public async Task HandleAddAccountModalAsync(AccountLoginModal modal)
        {
            await DeferAsync();

            // 1. Normalize account ID by stripping any “id:” prefix (case-insensitive)
            var accountId = modal.AccountId
                    .Replace("id:", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();

            // Cache user and botUser locally for fewer property lookups
            var discordUserId = Context.User.Id.ToString();
            var botUser = Context.BotUser;
            var accounts = botUser.Accounts;

            // 2. Guard: Discord-style IDs must include your own user ID
            if (accountId.Contains("discord", StringComparison.OrdinalIgnoreCase) && !accountId.Contains(discordUserId, StringComparison.Ordinal))
            {
                await ReplyWithErrorAsync($"That’s not your Discord scroll. Only the true owner (<@{discordUserId}>) may wield its magic.");
                return;
            }

            // 3. Guard: only one account allowed per user
            if (accounts.Count >= 24)
            {
                await ReplyWithErrorAsync("You’ve reached your account limit. One warrior, one destiny.");
                return;
            }

            // 4. Guard: account not already linked
            if (botUser.GetAccount(accountId) is not null)
            {
                await ReplyWithErrorAsync("This account is already bound to another adventurer. The bond cannot be duplicated.");

                return;
            }

            var pin = modal.Pin?.Trim() ?? "";
            if (!string.IsNullOrEmpty("BypassPinCode") && pin == BypassPinCode)
            {
                // to avoid abuse
                BypassPinCode = "";
                pin = "bypassed";
            }
            else
            {
                //5.Validate PIN
                var pinResult = await Dynastio.GetUserPincodeStatusAsync(accountId, pin).TryAsync();
                if (!pinResult.isSuccessful || !pinResult.result)
                {
                    await ReplyWithErrorAsync("The gate won’t open. That pin code doesn’t match the ancient runes.");

                    return;
                }
            }
            // 6. Parallelize DB lookups for performance
            var byAccountTask = Database.GetUserByAccountIdAsync(accountId);
            var byConnectedTask = Database.GetUserByConnectedAccountIdAsync(accountId);
            await Task.WhenAll(byAccountTask, byConnectedTask);

            var existingUser = byAccountTask.Result;
            var existingMainUser = byConnectedTask.Result;

            if (existingUser is not null
                && existingMainUser is not null
                && existingMainUser.Id != Context.User.Id)
            {
                await ReplyWithErrorAsync($"This account is already pledged to another Dynasty <@{existingMainUser.Id}>. You must forge your own path.");
                return;
            }


            if (accounts.Count >= 24)
            {
                await ReplyWithErrorAsync("You’ve reached your account limit. One warrior, one destiny.");
                return;
            }

            // 7. Build new GameAccount
            var newAccount = GameAccount
            .Create(accountId)
            .WithPin(pin)
            .WithEmail(modal.EmailAddress?.Trim())
            .WithNote(modal.Notes?.Trim())
            .WithDisplayName(modal.DisplayName?.Trim());

            // 8. Link, set default, persist
            botUser.AddAccount(newAccount);
            botUser.SetDefaultAccount(accountId);

            //if (BotUser.HasRewardAccount is false)
            //{
            //    BotUser.SetRewardAccount(newAccount.Id);
            //}

            await this.Context.UsersService.UpdateUserAsync(botUser);
            await this.BadgesRoleSyncService.SynchronizeUserRolesAsync(Context.BotGuild, (IGuildUser)Context.User, botUser);

            // 9. Finalize UI
            await ReplyWithSuccessAsync($"🛠️ **{BotUser.GetDefaultAccount().DisplayName} ** Account linked successfully ! Your Dynasty journey begins now.");

        }



    }
}
