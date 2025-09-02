using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Guild.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonRankingPrefixModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildsetuprankingmodule.prefix";

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
                .WithLabel("Change Roles Perfix")
                .WithEmote(module.EmoteService.GetEmoteByName("developer"))
                .WithStyle(ButtonStyle.Secondary)
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
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {

            TextInputBuilder? textInput = new TextInputBuilder()
               .WithCustomId("a")
               .WithLabel("Ranking Roles Prefix")
               .WithValue(BotGuild?.RankingSettings?.Prefix ?? "rank: ")
               .WithMinLength(3)
               .WithMaxLength(8)
               .WithStyle(TextInputStyle.Short);

            var modal = new ModalBuilder()
                .WithTitle("Change Ranking Roles Prefix")
                .WithCustomId(DiscordInput.GetCustomId(InteractionIdBase + "_inline"))
                .AddTextInput(textInput);

            await RespondWithModalAsync(modal.Build());

            ///     -----------------------------------------------------------------
            ///     
            var menu = await this.Context.WaitForContextModalAsync(TimeSpan.FromMinutes(2));
            if (menu is null)
                return;


            var data = (menu.Data as SocketModalData).Components.First()?.Value ?? "NOT_FOUND";
            if (data is null || data == "NOT_FOUND")
                return;

            data = data.Trim() + " ";
            if (!data.Contains(": ") && !data.EndsWith(": "))
            {
                await menu.RespondAsync("Prefix most ends with <: > examples:  <rank: > <score: > <level: >", ephemeral: true);
                return;
            }
            else
            {
                await menu.RespondAsync("Prefix updated and role list refreshed !", ephemeral: true);
            }

            BotGuild.UpdateXpSettings(a => a.Prefix = data);
            await GuildService.UpdateGuildAsync(Context.BotGuild);

            Context.IsDeferred = true;
            await GuildSetupRankingServiceModule.ExternalExecuteAsync(this);
        }
    }
}
