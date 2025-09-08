using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Modules.Guild;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;

using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Guild.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonMenuModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.menu";

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

                .WithLabel(module["buttons.interactions.menu.menu.label"])

                .WithEmote(module.EmoteService.GetEmoteByName("left_shop_icon1"))

                .WithStyle(ButtonStyle.Success)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: "menu"));
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
        // ===== Constants =====

        /// <summary>
        /// Slash command name for opening the menu.
        /// </summary>
        public const string SlashCommandName = "guild";


        // ===== Slash Command =====

        /// <summary>
        /// Displays the main Dynast.io navigation menu.
        /// </summary>
        [SlashCommand(SlashCommandName, "menu.description")]
        [ComponentInteraction(InteractionIdBase + ":*")]
        [RequireMessageComponentTimeout]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ShowMenuAsync()
        {
            if (!Context.IsDeferred)
                await DeferAsync(); 

      

            // 2️⃣ Build container with header
            var container = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
                .WithAccentColor(Color.DarkGreen)
         
                .WithSeparator(SeparatorSpacingSize.Small, true);

            // 5️⃣ Build and send final menu
            var components = new ComponentBuilderV2()
                .WithContainer(container);

            if ((User as IGuildUser).GuildPermissions.Administrator)
            {

                var containerAdmin = new ContainerBuilder()
                      //.WithMediaGallery(AssetUrlService[AssetType.])
                      .WithAccentColor(Color.DarkGreen)
                      .WithTextDisplay($"## {EmoteService.GetEmoteByName("administrator")} Administrator Menu")
                      .WithTextDisplay("Access powerful tools to configure and manage your server’s Dynast.io bot integration. Adjust roles, permissions, features, and automate server management to fit your community’s needs.")
                      .WithSeparator(SeparatorSpacingSize.Large, true);

                var sectionAdmin = new SectionBuilder()
                    .WithTextDisplay("Admin tools for managing and configuring your server’s Dynast.io bot module. Access advanced settings and controls to optimize your community’s experience.")
                    .WithAccessory(GuildSetupRankingServiceModule.BuildButton(this));

                containerAdmin.WithSection(sectionAdmin);

                var sectionBadgeSyncerModule = new SectionBuilder()
                    .WithTextDisplay("Sync in-game badges with Discord roles. Use this module to automatically assign roles based on players’ achievements in Dynast.io.")
                    .WithAccessory(GuildSetupBadgeSyncerServiceModule.BuildButton(this));

                containerAdmin.WithSection(sectionBadgeSyncerModule);

                components.WithContainer(containerAdmin);
            }


            components.WithActionRow([Menu.Buttons.ButtonCloseModule.BuildButton(this), GetDiscordButton(), GetTelegramButton()]);

            await ReplyOrModifyAsync(components: components.Build());
        }
    }
}
