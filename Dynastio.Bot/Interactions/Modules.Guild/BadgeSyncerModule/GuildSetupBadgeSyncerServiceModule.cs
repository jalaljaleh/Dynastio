using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Guild.Buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Guild
{
    /// <summary>
    /// The GuildSetupModule provides a menu for managing and configuring guild (server) settings.
    /// This menu allows server administrators to enable or disable features such as the ranking system,
    /// adjust badge settings, and access other guild-related configuration options.
    /// Use this module to streamline the setup and management of your Discord server's core features.
    /// </summary>
    public class GuildSetupBadgeSyncerServiceModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public const string InteractionIdFull = InteractionIdBase + IdParameterFormat;

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildetupBadgeSyncerModule";

        /// <summary>
        /// Suffix format appended after the base ID.
        /// {0} = page, {1} = page size, {2} = trigger context.
        /// Allows you to pass parameters through the button’s CustomId.
        /// </summary>
        public const string IdParameterFormat = ":{0}";

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------
        public static async Task ExternalExecuteAsync(MenuModulesBase module)
        {
            // find your next module by its base ID
            ComponentCommandInfo next = module.Context.InteractionService.ComponentCommands
                .FirstOrDefault(cmd => cmd.Name == GuildSetupBadgeSyncerServiceModule.InteractionIdBase);

            if (next != null)
            {
                await next.ExecuteAsync(module.Context, module.Context._services);
            }
            else
                await module.ReplyWithErrorAsync("❌ Could not find the next setup module.");
        }
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
                .WithLabel("Badge Syncer")
                .WithEmote(module.EmoteService.GetEmoteByName("left_build_icon"))
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
        [ComponentInteraction(InteractionIdBase)]
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync() => await ExecuteAsync("");


        [ComponentInteraction(InteractionIdBase + ":*")]
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            if (!Context.IsDeferred)
                await DeferAsync();

            var moduleBadgesSettings = Context.BotGuild.BadgeSettings;

            var container = new ContainerBuilder()
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
              .WithAccentColor(moduleBadgesSettings.IsEnabled ? Color.Green : Color.Red)
              .WithTextDisplay("Configure your server's core features and settings. Manage ranking, badges, and other modules to tailor your Discord guild experience. Use the options below to enable, disable, or adjust key server functionalities.")
              .WithSeparator(SeparatorSpacingSize.Large, true);




            var prefixSection = $"\n## **{EmoteService.GetEmoteByName("magichat")} Role Prefix:** ` {BotGuild.BadgeSettings.Prefix ?? "None"} ` ` {(BotGuild.BadgeSettings.IsEnabled ? "✅ Enabled" : "❌ Disabled")} `" +
                $"\nThis service automatically syncs your in-game badges with Discord roles. If you own a badge in the game, you will receive the corresponding role on this server. Keep your Discord roles up-to-date with your achievements!";

            var sectionRoles = new SectionBuilder()
                .WithTextDisplay(prefixSection)
                .WithAccessory(ButtonBadgeSyncerActivatorModule.BuildButton(this));

            container.WithSection(sectionRoles);


            var headerRole = RoleHelper.GetRoleAbovePrefix(Guild, BotGuild.BadgeSettings.Prefix);
            var roles = RoleHelper.GetRolesWithPrefix(this.Guild, BotGuild.BadgeSettings.Prefix);
            var badges = Enum.GetNames<BadgeType>();

            roles = roles
                .Where(
                a => 
                badges.Any(
                    b => 
                    b.Contains(a.Name.ToBadgeEnumAble(BotGuild.BadgeSettings.Prefix),StringComparison.OrdinalIgnoreCase)
                    )).ToList();

            var levelRoles =
                $"\n## **{EmoteService.GetEmoteByName("diamond")} Badge Roles**:\n" +
                $"\n### **Header Role**: {(headerRole != null ? headerRole.Mention : "Not Found")}" +
                $"\n{(roles != null && roles.Count > 0 ? string.Join(", ", roles.OrderByDescending(a => a.Position).Select(a => a.Mention)) : "None")}";

            var sectionRole = new SectionBuilder()
                .WithTextDisplay(levelRoles)
               .WithAccessory(ButtonBadgePrefixModule.BuildButton(this));

            container.WithSection(sectionRole);

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                 .WithContainer(container);

            await ModifyMenuMessageAsync(components: cb.Build());

        }


    }
}
