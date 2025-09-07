using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Modules.Menu.Modal;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using System;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class to add a "Search Players" button that
    /// opens the SearchPlayerModalForm. Requires Guild context.
    /// Implements IMenuComponentService to integrate with the menu system.
    /// </summary>
    [RequireContext(ContextType.Guild)]
    public class ButtonSearchPlayersModule : MenuModulesBase, IMenuComponentRule
    {
        //--------------------------------------------------------------------------------
        // SECTION: Constants and ID Formats
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Prefix for the CustomId on the "Search Players" button.
        /// Used by ComponentInteraction to route clicks here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.button.searchplayers";

        //--------------------------------------------------------------------------------
        // SECTION: IMenuComponentService Implementation
        //--------------------------------------------------------------------------------



        //--------------------------------------------------------------------------------
        // SECTION: Builder Method
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Builds the actual ButtonBuilder for the menu.
        /// Customize label, emote, style, and CustomId here.
        /// </summary>
        /// <param name="suffixArgs">
        /// Optional suffix arguments for your CustomId format.
        /// </param>
        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] suffixArgs)
        {
            return new ButtonBuilder()
                .WithLabel(module["buttons.interactions.menu.searchPlayers.label"])
                .WithEmote(module.EmoteService.GetEmoteByName("search"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId(InteractionIdBase);
        }

        //--------------------------------------------------------------------------------
        // SECTION: Interaction Handler
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Invoked when the "Search Players" button is clicked.
        /// Displays the SearchPlayerModalForm to gather filter inputs.
        /// </summary>
        [ComponentInteraction(InteractionIdBase)]
        [RequireMessageComponentTimeout]
        public async Task ModalSearchPlayers()
        {
            // Opens the modal defined by SearchPlayerModalForm
            await RespondWithModalAsync<SearchPlayerModalForm>(ButtonPlayersModule.ModalCustomId);
        }
    }
}
