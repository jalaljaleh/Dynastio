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
    public class GuildSetupRankingServiceModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public const string InteractionIdFull = InteractionIdBase + IdParameterFormat;

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.guild.buttons.guildsetuprankingmodule";

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
                .FirstOrDefault(cmd => cmd.Name == GuildSetupRankingServiceModule.InteractionIdBase);

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
                .WithLabel(module["ranking_module"])
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
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync() => await ExecuteAsync("");


        [ComponentInteraction(InteractionIdBase + ":*")]
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = "")
        {
            if (!Context.IsDeferred)
                await DeferAsync();

            var moduleRanking = Context.BotGuild.RankingSettings;
            var moduleBadgesSettings = Context.BotGuild.BadgeSettings;

            var error = ButtonRankingActivatorModule.ValidateRankingModule(moduleRanking, Context);
            if (error != null)
            {
                moduleRanking.IsEnabled = false;
            }

            var containerModules = new ContainerBuilder()
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
              .WithAccentColor(moduleRanking.IsEnabled ? Color.Green : Color.Red)
              .WithTextDisplay("Configure your server's core features and settings. Manage ranking, badges, and other modules to tailor your Discord guild experience. Use the options below to enable, disable, or adjust key server functionalities.")
              .WithSeparator(SeparatorSpacingSize.Large, true);

            // ---------------------- RANKING MODULE ----------------------
            var sectionRanking = new SectionBuilder()
                    .WithTextDisplay("## 🏆 Ranking Module")
                    .WithTextDisplay("Toggle the ranking system for your server. Click to enable or disable the ranking module based on your current settings.")
                    .WithAccessory(ButtonRankingActivatorModule.BuildButton(this));

            containerModules.WithSection(sectionRanking);

            containerModules.WithSeparator(SeparatorSpacingSize.Large, false);


            string text =

            $"\n## **{EmoteService.GetEmoteByName("robot")} Ranking Status**: ` {(moduleRanking.IsEnabled ? "✅ Enabled" : "❌ Disabled")} `" +
            $"\n### **{EmoteService.GetEmoteByName("swamppotion")} Base XP per Message**: ` {moduleRanking.BaseXpPerMessage} `" +
            $"\n### {EmoteService.GetEmoteByName("firepotion")} **Random XP Bonus**: ` {moduleRanking.RandomXpBonus} `  " +
            $" {EmoteService.GetEmoteByName("regenerationpotion")} **Booster XP**: ` {moduleRanking.BoosterXp} `" +
            "";

            containerModules.WithTextDisplay(text);



            ComponentBuilderV2 cb = new ComponentBuilderV2()
                    .WithContainer(containerModules);

            if (error != null)
                cb.WithContainer(new ContainerBuilder()
                    .WithAccentColor(Color.Red)
                    .WithTextDisplay(error));

            cb
                    .WithContainer(GetChannelsContainer())
                    .WithContainer(GetRolesContainer())
                .WithContainer(GetGameRewardContainer());

            cb.WithActionRow([GuildSetupRankingServiceModule.BuildButton(this)]);

            await ReplyOrModifyAsync(components: cb.Build());

        }
        public ContainerBuilder GetGameRewardContainer()
        {
            var container = new ContainerBuilder()
                .WithAccentColor(BotGuild.RankingSettings.IsGameRewardEnabled ? Color.Green : Color.Red);

            var sectionGameReward = new SectionBuilder()

                .WithTextDisplay(
            $"\n## **{EmoteService.GetEmoteByName("coin")} Game Reward**: ` {(BotGuild.RankingSettings.IsGameRewardEnabled ? "✅ Enabled" : "❌ Disabled")} `" +
            $"\n The Game Reward module allows you to grant in-game coins to users when they level up in your Discord server. When enabled, each time a user reaches a new level, they will automatically receive coins in the game.The amount of coins awarded is based on the user's current level, providing greater rewards for higher achievements. Use this module to incentivize participation and progression within your community." +
            $"\n## Total Rewards: {EmoteService.GetEmoteByName("shop_coins_icon_5")} ` {DynastioHelper.TotalRankingReward.ToMetric()} coins` " +
            $"\n{DynastioHelper.TabledLevelRewards}" +
            $"")

                .WithAccessory(ButtonRankingGameRewardModule.BuildButton(this));

            container.WithSection(sectionGameReward);

            return container;
        }
        public ContainerBuilder GetChannelsContainer()
        {
            var container = new ContainerBuilder();

            container.WithTextDisplay(
                "### :dart: XP Message Channels\n" +
                "Select which public channels will grant XP to users when they send messages. Only messages in these channels will contribute to user ranking."
            );
            container.WithActionRow([DropdownRankingChannelsModule.BuildSelectMenu(this)]);

            container.WithSeparator(SeparatorSpacingSize.Large, true);

            container.WithTextDisplay(
                "### :bell: Level-Up Logger Channel\n" +
                "Choose a public channel where notifications about users leveling up and earning rewards will be posted. This helps keep your community informed about member progress."
            );
            container.WithActionRow([DropdownRankingLoggerChannelModule.BuildSelectMenu(this)]);

            return container;
        }
        public ContainerBuilder GetRolesContainer()
        {
            var roles = RoleHelper.GetRolesWithPrefix(this.Guild, BotGuild.RankingSettings.Prefix);

            var container = new ContainerBuilder()
                .WithAccentColor(BotGuild.RankingSettings.IsRankingRoleAssignmentEnabled ? Color.Green : Color.Red);

            var prefixSection = $"\n## **{EmoteService.GetEmoteByName("magichat")} Role Prefix:** ` {BotGuild.RankingSettings.Prefix ?? "None"} ` ` {(BotGuild.RankingSettings.IsRankingRoleAssignmentEnabled ? "✅ Enabled" : "❌ Disabled")} `" +
                         $"\nWhen users level up in your Discord server, they are automatically assigned roles based on their current level.   As users progress, they receive higher - ranked roles corresponding to their level. The system supports up to 40 distinct level roles, allowing for granular recognition of user achievements.For more details, refer to the Dynastio Bot Ranking Service **[Documentation](https://github.com/jalaljaleh/Dynastio.Bot/blob/master/Dynastio.Bot/Services/XpRankingSystem/XpRankingSystemService.md)**";


            var sectionRoles = new SectionBuilder()
                .WithTextDisplay(prefixSection)
                .WithAccessory(ButtonRankingRoleAssignmentModule.BuildButton(this));



            container.WithSection(sectionRoles);
            container.WithSeparator(SeparatorSpacingSize.Small, false);

            var searchLevelRoles = $"\n## {EmoteService.GetEmoteByName("shop_skins_group_icon")} Role Search Result: ` {roles.Count} `";
            var searchLevelRolesDescription = "Role names must begin with a specific prefix, such as <rank: >, to ensure proper identification and management. The assignment process is determined by the position of each role in the server's role hierarchy, starting from the lowest position and moving upwards.";

            container.WithSection(new SectionBuilder()
                .WithTextDisplay(searchLevelRoles)
                .WithTextDisplay(searchLevelRolesDescription)
                .WithAccessory(ButtonRankingPrefixModule.BuildButton(this))
                );

            var headerRole = RoleHelper.GetRoleAbovePrefix(Guild, BotGuild.RankingSettings.Prefix);
            var levelRoles =
                $"\n## **{EmoteService.GetEmoteByName("diamond")} Level Roles**:\n" +
                $"\n### **Header Role**: {(headerRole != null ? headerRole.Mention : "Not Found")}" +
                $"\n{(roles != null && roles.Count > 0 ? string.Join(", ", roles.OrderByDescending(a => a.Position).Take(5).Select(a => a.Mention)) : "None")} more ...";

            container.WithTextDisplay(levelRoles);


            return container;
        }
    }
}
