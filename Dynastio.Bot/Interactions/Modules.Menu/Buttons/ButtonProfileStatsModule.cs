using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonProfileStatsModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.profileStats";

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
                .WithLabel("Stats: " + args.FirstOrDefault().ToString())
                .WithEmote(module.EmoteService.GetEmoteByName("unknown"))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: args.FirstOrDefault().ToString()));
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
        [RequireLinkedAccount]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            await DeferAsync();

            var profile = Context.BotUser.GetDefaultAccount();
            var stat = await profile.GetCachedProfileStatAsync(Context.Dynastio);
            if (stat == null)
            {
                await ReplyWithErrorAsync("Can't find your stat !");
                return;
            }

            string content = trigger switch
            {
                "kill" => string.Join(" | ", stat.Kill.OrderByDescending(a=>a.Value).Select(a => " " + EmoteService.GetEmote(a.Key).ToString() + $" ` {a.Value.ToMetric()} `")),
                "gather" => string.Join(" | ", stat.Gather.OrderByDescending(a => a.Value).Select(a => " " + EmoteService.GetEmote(a.Key).ToString() + $" ` {a.Value.ToMetric()}`")),
                "death" => string.Join(" | ", stat.Death.OrderByDescending(a => a.Value).Select(a => " " + EmoteService.GetEmote(a.Key).ToString() + $" ` {a.Value.ToMetric()}`")),
                "craft" => string.Join(" | ", stat.Craft.OrderByDescending(a => a.Value).Select(a => " " + EmoteService.GetEmote(a.Key).ToString() + $" ` {a.Value.ToMetric()}`")),
                _ => "not found"
            };

            if (content.Length > 3800)
                content = content.Substring(0, 3800);

            var containerb = new ContainerBuilder()
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
            //  .WithAccentColor(Color.Green)
              .WithTextDisplay($"# {trigger} Stats \n#" + content);
              //.WithActionRow([
              //    BuildButton(this,"kill"),
              //    BuildButton(this,"gather"),
              //    BuildButton(this,"death"),
              //    BuildButton(this,"craft")]);

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb);

            await ReplyOrModifyAsync(components: cb.Build());

        }
    }
}
