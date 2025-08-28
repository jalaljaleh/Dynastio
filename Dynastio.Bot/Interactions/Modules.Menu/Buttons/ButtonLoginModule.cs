using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
using Dynastio.Bot.Interactions.Modules.Menu.Modal;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Extenstions;
using Microsoft.Extensions.DependencyInjection;
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
        [ComponentInteraction(InteractionIdBase + "")]
        public async Task ExecuteAsync()
        {
            await RespondWithModalAsync<AccountLoginModal>(InteractionModalId);
        }


        /// <summary>
        /// Receive modal submissions from SearchPlayerModalForm.
        /// Forwards values to main ExecuteAsync.
        /// </summary>
        [ModalInteraction(InteractionModalId)]
        public async Task HandleModalAsync(AccountLoginModal modal)
        {
            await DeferAsync();
            string id = modal.AccountId.Remove("id:", "Id:", "ID:", "iD:").Trim(); // don't use tolower

            string errorText = "";
            // check if its his own discord user account
            if (id.Contains("discord") && !id.Contains(Context.User.Id.ToString()))
            {
                errorText = "its not your account";
                return;
            }

            if (Context.BotUser.Accounts.Count >= 1)
            {
                errorText = "can't add more than 1 account .";
                return;
            }

            if (Context.BotUser.GetAccount(id) != null)
            {
                errorText = "this account added already !";
                return;
            }


            bool authorized = await Dynastio.GetUserPincodeStatusAsync(id, modal.Pin.Trim());
            if (authorized is false)
            {
                errorText = "wrong pin code !";
                return;
            }

            var targetUser = await this._db.GetUserByAccountIdAsync(id);
            var targetMainAccount = await this._db.GetUserByConnectedAccountIdAsync(id);
            if (targetUser != null && targetMainAccount != null)
            {
                if (targetMainAccount.Id != Context.User.Id)
                {
                    errorText = "this account is connected by another user !";
                    return;
                }
            }

            var account = new UserGameAccount()
            {
                Id = id,
                AddedAt = DateTime.UtcNow,
                IsDefault = true,
                PinCode = modal.Pin.Trim(),
                Email = modal.EmailAddress,
                Reminder = modal.Notes
            }.SetReminder(form.Reminder);

            Context.BotUser.Accounts.Add(account);
            Context.BotUser.SwitchAccount(ref account);

            await dynastioBotDatabase.UpdateAsync(Context.BotUser);

            await ModifyCurrentMessageAsync(userMention, embed: this["modal.dynastio.accounts.add.succeeful.description"].ToEmbed(this["modal.dynastio.accounts.add.succeeful.title"], Color.Green));

            // await _guildService.SyncUserBadges(Context.BotUser);

        }
    }
}
