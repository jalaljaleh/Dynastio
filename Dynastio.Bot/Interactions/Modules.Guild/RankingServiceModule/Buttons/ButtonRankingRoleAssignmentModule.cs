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
    public class ButtonRankingRoleAssignmentModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildsetuprankingmodule.roleassignment";

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
                .WithLabel(module["buttons.interactions.guildsetuprankingmodule.roleassignment.enable.label"])
                .WithEmote(module.EmoteService.GetEmoteByName("developer"))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));

            if (module.Context.BotGuild.RankingSettings.IsRankingRoleAssignmentEnabled)
            {
                btn
                     .WithLabel(module["buttons.interactions.guildsetuprankingmodule.roleassignment.disable.label"])
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
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            if (Context.BotGuild.RankingSettings.IsRankingRoleAssignmentEnabled == false)
            {
                var roles = RoleHelper.GetRolesWithPrefix(this.Guild, BotGuild.RankingSettings.Prefix);

                if (roles.Count > 0)
                    await RespondAsync("Discord roles assignment are now enabled ! Users will get new roles each time they level up.", ephemeral: false);
                else
                {
                    await RespondAsync("No any matched role with this prefix found !", ephemeral: false);
                    return;
                }
                BotGuild.UpdateXpSettings(a => a.IsRankingRoleAssignmentEnabled = true);
            }
            else
            {
                await RespondAsync("Discord roles assignment have been disabled. Users will no longer receive roles for leveling up.", ephemeral: false);

                BotGuild.UpdateXpSettings(a => a.IsRankingRoleAssignmentEnabled = false);
            }

            await GuildService.UpdateGuildAsync(BotGuild);


            Context.IsDeferred = true;
            await GuildSetupRankingServiceModule.ExternalExecuteAsync(this);
        }

    }
}
