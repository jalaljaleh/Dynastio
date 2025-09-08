using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
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
    public class BUttonPersonalChestModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public DynastioApi Dynastio { get; set; }

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.personalchest";

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

                .WithLabel(module["buttons.interactions.menu.privatechest.label"])

                .WithEmote(module.EmoteService.GetEmoteByName("privatechest"))

                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));
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
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            await DeferAsync();

            var account = Context.BotUser.GetDefaultAccount();
            var chest = await account.GetCachedPersonalChestAsync(Dynastio);

            var shape = new DynastioShapeGenerator(EmoteService).GeneratePersonalChest(chest);

            var sectionAccount = new SectionBuilder()
                           .WithAccessory(new ThumbnailBuilder(User.TryGetAvatarUrl()))
                        // .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("privatechest").Url))
                        .WithTextDisplay(
                          $"# {EmoteService.GetEmoteByName("privatechest")} Personal Chest {account.DisplayName} \n" +
                          shape);


            var container = new ContainerBuilder()
              .WithAccentColor(Color.Green)
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
              .WithTextDisplay($"You logined as {account.DisplayName} Peek into your Dynast.io legacy — see your linked account, level, score, badges, and more. Every survivor has a story… this is yours.")

              .WithSeparator(SeparatorSpacingSize.Large, true)
              .WithSection(sectionAccount);



            // Constants for easy tuning
            const int TotalButtons = 20;
            const int ButtonsPerRow = 5;
            int totalRows = TotalButtons / ButtonsPerRow;

            // Cache your dictionary once
            var items = chest.GetAsDictionary();

            // should separator ? add if you need it
            container.WithSeparator(SeparatorSpacingSize.Small, true);

            for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
            {
                var actionRow = new ActionRowBuilder();

                for (int colIndex = 0; colIndex < ButtonsPerRow; colIndex++)
                {
                    int currentIndex = rowIndex * ButtonsPerRow + colIndex;

                    items.TryGetValue(currentIndex, out PersonalChestItem item);

                    var button = ButtonPersonalChestItemModule.BuildButton(this, item);
                    actionRow.WithButton(button);
                }

                // Attach the completed row
                container.WithActionRow(actionRow);
            }

            var component = new ComponentBuilderV2()
                .WithContainer(container)
                ;
            await ReplyOrModifyAsync(components: component.Build());
        }
    }
}
