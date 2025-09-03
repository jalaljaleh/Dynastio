using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
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
    public class ButtonRankingActivatorModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public InteractionService interactionService { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildsetuprankingmodule.activator";

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
                .WithLabel(module["enable_module"])
                .WithEmote(module.EmoteService.GetEmoteByName("developer"))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));



            if (module.BotGuild.RankingSettings.IsEnabled)
            {
                btn
                    .WithLabel(module["all_set"])
                    .WithStyle(ButtonStyle.Success);
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
            await DeferAsync();
            Context.IsDeferred = true;

            var module = BotGuild.RankingSettings;

            // 1) If it’s already on, turn it off and exit
            if (module.IsEnabled)
            {
                module.IsEnabled = false;
                await ReplyWithSuccessAsync("✅ Ranking module has been disabled.");
                return;
            }

            // 2) Run all validations in one pass
            var error = ValidateRankingModule(module, Context);
            if (error != null)
            {
                await ReplyWithErrorAsync(error);
                return;
            }
            module.IsEnabled = true;

           await GuildService.UpdateGuildAsync(Context.BotGuild);

            // 3) All good – move on to the next setup step
            await GuildSetupRankingServiceModule.ExternalExecuteAsync(this);
        }

        public static string ValidateRankingModule(RankingSettings module, BotSocketInteractionContext Context)
        {
            if (module.IsRankingRoleAssignmentEnabled && string.IsNullOrWhiteSpace(module.Prefix) || !module.Prefix.Contains(": ") && !module.Prefix.EndsWith(": "))
            {
                return "❌ Ranking role prefix is not set, but role assignment is enabled.";
            }

            if (!RoleHelper.GetRolesWithPrefix(Context.Guild, module.Prefix).Any())
            {
                return "❌ Ranking role prefix conflicts with an existing role name.";
            }

            if (module.AllowedXpChannelIds == null || !module.AllowedXpChannelIds.Any())
                return "❌ No allowed XP channels have been set.";

            // -------- Logger Channel ----------

            if (module.RankingLogChannelId <= 0)
                return "❌ Ranking log channel ID must be a positive integer.";

            // Direct lookup is faster than LINQ over .Channels
            var channel = Context.Guild.GetChannel(module.RankingLogChannelId);
            if (channel == null)
                return "❌ Ranking log channel ID is invalid or does not exist.";

            // -----------------------------------

            if (module.BaseXpPerMessage < 1)
                return "❌ Base XP per message must be at least 1.";

            if (module.MessageScoreCooldownSeconds < 1)
                return "❌ Message score cooldown must be at least 1 second.";

            return null;
        }



    }
}

