using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using static Dynastio.Bot.Interactions.Modules.Owner.Developer;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonShapeGeneratorModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.shapeGenerator";

        /// <summary>
        /// Suffix format appended after the base ID.
        /// {0} = page, {1} = page size, {2} = trigger context.
        /// Allows you to pass parameters through the button’s CustomId.
        /// </summary>
        public const string IdParameterFormat = ":{0}";

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------

        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] args)
        {
            var btn = new ButtonBuilder()
                .WithLabel("Shape Generator")
                .WithEmote(module.EmoteService.GetEmoteByName("ball"))
                .WithStyle(ButtonStyle.Primary)
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
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            var shapeGen = new DynastioShapeGenerator(EmoteService);
            int size = Common.Random.Next(2, 6);
            bool type = Common.Random.Next(2) == 0;

            string shape = type switch
            {
                false => await shapeGen.CreateRandomShapeAsync<EntityType>(size, size),
                true => await shapeGen.CreateRandomShapeAsync<ItemType>(size, size),
            };

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithTextDisplay("# Shape Generator")
                .WithTextDisplay(shape);

            await ReplyOrModifyAsync(components: cb.Build());

        }
    }
}
