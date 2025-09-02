using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot.Interactions.Modules.Guild.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonBadgeSyncerActivatorModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildetupBadgeSyncerActivator";

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
                .WithLabel("Enable")
                .WithEmote(module.EmoteService.GetEmoteByName("developer"))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));

            if (module.BotGuild.BadgeSettings.IsEnabled)
            {
                btn.WithLabel("Disable")
                    .WithStyle(ButtonStyle.Danger);
            }
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

            if (BotGuild.BadgeSettings.IsEnabled)
            {
                BotGuild.UpdateBadgeSettings(a => a.IsEnabled = false);
                await RespondAsync("Badge Sync Service disabled !");
            }
            else
            {
                if (string.IsNullOrEmpty(BotGuild.BadgeSettings.Prefix))
                {
                    await ReplyWithErrorAsync("Error: Prefix can't be null");
                    return;
                }
                var headerRole = RoleHelper.GetRoleAbovePrefix(Guild, BotGuild.BadgeSettings.Prefix);
                if (headerRole is null)
                {
                    await ReplyWithErrorAsync("Error: Header role not found !");
                    return;
                }
                var roles = RoleHelper.GetRolesWithPrefix(Guild, BotGuild.BadgeSettings.Prefix);
                if (roles is null || roles.Any() is false)
                {
                    await ReplyWithErrorAsync("Error: no any matched role found for badges !");
                    return;
                }
                BotGuild.UpdateBadgeSettings(a => a.IsEnabled = true);
                    await RespondAsync("Badge Sync Service enabled !");
                
            }
            await GuildService.UpdateGuildAsync(Context.BotGuild);


            Context.IsDeferred = true;
            await GuildSetupBadgeSyncerServiceModule.ExternalExecuteAsync(this);

        }
    }
}
