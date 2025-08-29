using Discord;
using Discord.Interactions;
using Dynastio.Bot;
using Dynastio.Bot.Interactions.Modules.Menu.Buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu
{

    /// <summary>
    /// Slash command handler for the Dynast.io main menu.
    /// Displays a navigational hub with both public and personal commands.
    /// </summary>
    [RequireContext(ContextType.Guild)]
    [RequireTeam]
    public class MenuModule : MenuModulesBase
    {
        // ===== Constants =====

        /// <summary>
        /// Slash command name for opening the menu.
        /// </summary>
        public const string CommandName = "menu";


        // ===== Slash Command =====

        /// <summary>
        /// Displays the main Dynast.io navigation menu.
        /// </summary>
        [SlashCommand(CommandName, "Opens the central Dynast.io menu hub.")]
        [ComponentInteraction(CommandName)]
        public async Task ShowMenuAsync()
        {
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
               .WithAccessory(ButtonProfileModule.BuildButton(this));
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
                .WithSeparator(SeparatorSpacingSize.Small, true);

            if (BotUser.HasLinkedAccount)
            {
                container
                   .WithTextDisplay("Your own private commands, Custom‑fit components, tuned to your level and playstyle.")
                  // .WithActionRow([ButtonUserProfileModule.BuildButton(this)])
                  //.WithActionRow([Personalchest.BuildButton(this)])
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

            await FollowupAsync(components: components.Build());
        }
    }
}
