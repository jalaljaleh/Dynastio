using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
using Dynastio.Bot.Interactions.Modules.Menu.Modal;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Extenstions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Emit;
using System.Threading.Tasks;

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
        public BadgesRoleSyncService BadgesRoleSyncService { get; set; }
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
            return new ButtonBuilder()

                .WithLabel("Login")

                .WithEmote(module.EmoteService.GetEmoteByName("privatechest"))

                .WithStyle(ButtonStyle.Success)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: Guid.NewGuid().ToString()));
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
        public async Task ExecuteAsync(string trigger="")
        {
            if (Context.BotUser.HasLinkedAccount is false)
                await RespondWithModalAsync<AccountLoginModal>(InteractionModalId);
            else
                await this.ReplyWithErrorAsync("You've already linked your account. No need to craft another identity!");
        }


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
            if (accountId.Contains("discord", StringComparison.OrdinalIgnoreCase)
                && !accountId.Contains(discordUserId, StringComparison.Ordinal))
            {
                await ReplyWithErrorAsync("That’s not your Discord scroll. Only the true owner may wield its magic.");
                return;
            }

            // 3. Guard: only one account allowed per user
            if (accounts.Count >= 1)
            {
                await this.ReplyWithErrorAsync("You've already linked your account. No need to craft another identity!");
                return;
            }

            // 4. Guard: account not already linked
            if (botUser.GetAccount(accountId) is not null)
            {
                await ReplyWithErrorAsync("This account is already bound to another adventurer. The bond cannot be duplicated.");

                return;
            }

            var pin = modal.Pin?.Trim() ?? "";
            goto Label;
            // 5. Validate PIN
            var pinResult = await Dynastio.GetUserPincodeStatusAsync(accountId, pin).TryAsync();
            if (!pinResult.isSuccessful || !pinResult.result)
            {
                await ReplyWithErrorAsync("The gate won’t open. That pin code doesn’t match the ancient runes.");

                return;
            }
        Label:
            // 6. Parallelize DB lookups for performance
            var byAccountTask = _db.GetUserByAccountIdAsync(accountId);
            var byConnectedTask = _db.GetUserByConnectedAccountIdAsync(accountId);
            await Task.WhenAll(byAccountTask, byConnectedTask);

            var existingUser = byAccountTask.Result;
            var existingMainUser = byConnectedTask.Result;

            if (existingUser is not null
                && existingMainUser is not null
                && existingMainUser.Id != Context.User.Id)
            {
                await ReplyWithErrorAsync("This account is already pledged to another Dynasty. You must forge your own path.");
                return;
            }


            if (accounts.Count >= 1)
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

            await this.Context.UsersService.UpdateUserAsync(botUser);
            await this.BadgesRoleSyncService.SynchronizeUserRolesAsync(Context.BotGuild, (IGuildUser)Context.User, botUser);

            // 9. Finalize UI
            await ReplyWithSuccessAsync("🛠️ Account linked successfully ! Your Dynasty journey begins now.");

        }



    }
}
