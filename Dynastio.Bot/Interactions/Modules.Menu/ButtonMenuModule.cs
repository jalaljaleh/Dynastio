using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Modules.Guild;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;

using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
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
        public const string InteractionIdBase = "interactions.menu.buttons.menu";

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
        public const string SlashCommandName = "menu";


        // ===== Slash Command =====

        /// <summary>
        /// Displays the main Dynast.io navigation menu.
        /// </summary>
        [SlashCommand(SlashCommandName, "menu.description")]
        [ComponentInteraction(InteractionIdBase + ":*")]
        [RequireMessageComponentTimeout]
        [RequireContext(ContextType.Guild)]
        public async Task ShowMenuAsync()
        {
            if (!Context.IsDeferred)
                await DeferAsync(); // Acknowledge interaction early

            // 1️⃣ Header section
            var headerSection = new SectionBuilder()
                   .WithTextDisplay(
                   "# Dynast.io Bot   \n" +
                    $"**Welcome** {UserMention} this is the central nexus of your [Dynast.io](https://dynast.io/) journey. " +
                    "It’s your all‑in‑one hub to view your profile, personal chest, stats, manage settings, " +
                    "and keep everything running smoothly.\n\n")
             .WithAccessory(new ThumbnailBuilder(this.User.TryGetAvatarUrl(), "Dynast.io Bot", false));

            var profileButtonSection = new SectionBuilder();
            if (BotUser.HasLinkedAccount)
            {
                profileButtonSection.WithTextDisplay("Open your bot profile settings !")
               .WithAccessory(ButtonLoginModule.BuildButton(this));
            }
            else
            {
                profileButtonSection
                  .WithTextDisplay("Login to your game account !")
                     .WithAccessory(ButtonLoginModule.BuildButton(this));

            }
            //var rankButtonSection = new SectionBuilder()
            //   .WithTextDisplay("Open your ranking menu !")
            //   .WithAccessory(ButtonsService.GetButton(BtnType.Settings, "e4"));
            //var settingsButtonSection = new SectionBuilder()
            //    .WithTextDisplay("Open the Dynast.io Bot settings menu to personalize your experience !")
            //    .WithAccessory(ButtonsService.GetButton(BtnType.Settings, "e5"));


            // 2️⃣ Build container with header
            var container = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
                .WithAccentColor(Color.DarkGreen)
                .WithSection(headerSection)
                .WithSection(profileButtonSection)
                //.WithSection(rankButtonSection)
                //.WithSection(settingsButtonSection)
                .WithSeparator(SeparatorSpacingSize.Small, true);

            // 3️⃣ Public commands section
            //var publicSection = new SectionBuilder()
            //    .WithTextDisplay("Public controls for dynast.io — keeping everyone connected and in sync.")
            //    .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("shop_skins_group_icon").Url));


            container
                .WithActionRow(
                [ButtonPlayersModule.BuildButton(this),
                 ButtonServersModule.BuildButton(this),
                 ButtonTeamsModule.BuildButton(this),
                 ButtonSearchPlayersModule.BuildButton(this)])

                .WithActionRow([Interactions.Modules.Owner.ItemsModule.BuildButton(this)])
                .WithSeparator(SeparatorSpacingSize.Small, true);

            if (BotUser.HasLinkedAccount)
            {
                container
                   .WithTextDisplay("Your own private commands, Custom‑fit components, tuned to your level and playstyle.")
                   .WithActionRow([
                       ButtonProfileModule.BuildButton(this, "1"),
                       BUttonPersonalChestModule.BuildButton(this),
                       ButtonRankModule.BuildButton(this)])
                  //.WithActionRow([stat.BuildButton(this)])
                  //.WithActionRow([rank.BuildButton(this)])
                  // .WithTextDisplay("");
                  ;
            }
            else
            {
                container
                  .WithTextDisplay("Login to unlock your own private commands, Custom‑fit components, tuned to your level and playstyle.")
                  .WithActionRow([ButtonLoginModule.BuildButton(this)]);
            }


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

            components.WithActionRow([ButtonCloseModule.BuildButton(this),GetDiscordButton(), GetTelegramButton()]);

            await ReplyOrModifyAsync(components: components.Build());
        }
    }
}
