using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;

using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonRankModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.rank";

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

                .WithLabel(module["buttons.interactions.menu.rank.label"])

                .WithEmote(module.EmoteService.GetEmoteByName("tab_leaders_icon_active"))

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
            // Acknowledge the interaction to avoid the “This interaction failed” message
            await DeferAsync();

            var guildUser = Context.User as IGuildUser;
            var botUser = Context.BotUser;
            var account = Context.BotUser.GetDefaultAccount();
            var guildProfile = BotUser.GetOrCreateGuildProfile(Guild.Id);


            var content = BotGuild.RankingSettings.IsEnabled == false
                ? "This guild is not supporting the Ranking service module !"
                : $"## {new Emoji(":first_place:")} **{account.DisplayName}** Xp Ranking \n" +
                 $"### {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")} Current Level \t  ` Level {guildProfile.Level} `\t\t ` Xp {guildProfile.Xp} `   \n" +
                 $"### {EmoteService.GetEmoteByName("zoom_in")} Next Level  \t\t  `require {XpCalculator.GetLevelUpRequirementXp(guildProfile.Level, guildProfile.Xp)} Xp `\n" +
                 $"### {EmoteService.GetEmoteByName("shop_coins_icon_3")} Next Reward \t {EmoteService.GetEmoteByName("coin")} ` {XpCalculator.GetLevelCoinsReward(guildProfile.Level + 1)} Coins `  {EmoteService.GetEmoteByName("select_skin_button")} ` @{RoleHelper.GetNextRoleWithPrefix(User as IGuildUser, BotGuild.RankingSettings.Prefix)?.Name ?? "Not Found"} Role `\n" +
                 $"";


            var sectionRank = new SectionBuilder()
                  .WithAccessory(new ThumbnailBuilder(User.TryGetAvatarUrl()))
                  .WithTextDisplay(content);

            var container = new ContainerBuilder()
                .WithAccentColor(Color.Green)
                .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
                .WithTextDisplay("Peek into your Dynast.io legacy — see your linked account, level, score, badges, and more.")
                .WithSeparator(SeparatorSpacingSize.Large, true)
                .WithSection(sectionRank);

            if (BotGuild.RankingSettings.IsEnabled)
            {
                // 2. Gather all badge roles and user’s badges
                var allRankRoles = RoleHelper
                    .GetRolesWithPrefix(Context.Guild, BotGuild.RankingSettings.Prefix)
                    .OrderBy(r => r.Position)
                    .ToList();

                var userRankRoles = allRankRoles
                    .Where(r => (User as IGuildUser).RoleIds.Contains(r.Id))
                    .ToList();

                var lockedRankRoles = allRankRoles
                    .Except(userRankRoles)
                    .ToList();

                // 3. Find the “next” role (first locked in your sorted list)
                var nextBadge = lockedRankRoles.FirstOrDefault();

                // 4. Build the summary text
                var rankSummary = new StringBuilder()
                    .AppendLine($"**Roles:** {userRankRoles.Count}/{allRankRoles.Count}")
                    .AppendLine()
                    .AppendLine($"**Owned:** ` {userRankRoles.Count} ` ")
                    //            (userRankRoles.Any()
                    //              ? string.Join(" ", userRankRoles.Take(15).Select(r => r.Mention)) + " More ..."
                    //              : "None"))
                    //.AppendLine()
                    .AppendLine($"**Locked:** ` {lockedRankRoles.Count} ` ")
                    .AppendLine()
                    //.AppendLine($"**Next role:** " +
                    //            (nextBadge != null
                    //              ? $"{nextBadge.Mention} (requires Level {BotUser.GetOrCreateGuildProfile(Guild.Id).Level + 1})"
                    //              : "All roles unlocked!"))
                    .ToString();

                // 5. Inject into your embed/container
                var sectionRanks = new SectionBuilder()
                    .WithTextDisplay(rankSummary)
                      .WithAccessory(ButtonSyncRankRolesModule.BuildButton(this));


                container.WithSeparator(SeparatorSpacingSize.Large, true)
                    .WithSection(sectionRanks);
            }
            else
            {
                // 5. Inject into your embed/container
                var sectionRanks = new SectionBuilder()
                    .WithTextDisplay("This guild is not supporting service module !")
                      .WithAccessory(ButtonSyncRankRolesModule.BuildButton(this));

                container.WithSeparator(SeparatorSpacingSize.Large, true)
                    .WithSection(sectionRanks);
            }

            await ModifyMenuMessageAsync(components: new ComponentBuilderV2()
                .WithContainer(container)
                // .WithActionRow([ButtonCloseModule.BuildButton(this)])
                .Build());

        }
    }
}
