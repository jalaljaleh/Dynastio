using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Bot.Services.XpRankingSystem;
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
    public class ButtonProfileModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.userprofile";

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

                .WithLabel("Profile")

                .WithEmote(module.EmoteService.GetEmoteByName("left_team_icon"))

                .WithStyle(ButtonStyle.Success)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: "header"));
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
        public async Task ExecuteAsync(string trigger = null)
        {
            await DeferAsync();

            var discordUser = Context.User;
            var botUser = Context.BotUser;
            var account = Context.BotUser.GetDefaultAccount();
            var guildProfile = BotUser.GetOrCreateGuildProfile(Guild.Id);

            var sectionProfile = new SectionBuilder()
                .WithAccessory(new ThumbnailBuilder(Context.User.TryGetAvatarUrl()))
                .WithTextDisplay($"# {discordUser.Username}")
                .WithTextDisplay("Peek into your Dynast.io legacy — see your linked account, level, score, badges, and more. Every survivor has a story… this is yours.");


            var sectionRank = new SectionBuilder()
                 .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("tab_leaders_icon_active").Url))
              .WithTextDisplay(
                $"## {new Emoji(":first_place:")} Xp Ranking \n" +
                $"### {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")} Current Level \t ` Level {guildProfile.Level} `\t\t ` Xp {guildProfile.Xp} `   \n" +
                $"### {EmoteService.GetEmoteByName("zoom_in")} Next Level  \t\t ` Xp Requirement {XpCalculator.GetLevelUpRequirementXp(guildProfile.Level, guildProfile.Xp)} `\n" +
                $"### {EmoteService.GetEmoteByName("shop_coins_icon_3")} Next Reward  \t ` {XpCalculator.GetLevelCoinsReward(guildProfile.Level + 1)} Coins` | `Role: @{RoleHelper.GetNextRankingHigherRole(User as IGuildUser, BotGuild.XpSystemSettings.RankingRolePrefix)?.Name ?? "Not Found"} `\n"
                );

            var profile = await Dynastio.GetUserProfileAsync(account.Id);

            profile.UnlockedSkins.AddRange([SkinType.Ninja, SkinType.Snowman, SkinType.Anime, SkinType.Girl]);
            profile.Badges.AddRange([BadgeType.Administrator, BadgeType.CupBronze, BadgeType.Monthly]);


            string badges = string.Join("", profile.Badges.Select(a => EmoteService.GetEmote(a)));
            string unlockedSkins = string.Join("", profile.UnlockedSkins.Select(a => EmoteService.GetEmoteByName("skin_" + a)));


            var sectionAccount = new SectionBuilder()
                 .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("privatechest").Url))
              .WithTextDisplay(
                $"# :crossed_swords: {account.DisplayName} \n" +
                $"You logined as {account.DisplayName} profile details are here." +
                $"# {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")}  `Level {profile.Level} `\t{EmoteService.GetEmoteByName("coin")} `Coins {profile.Coins.ToMetric()}` \n" +
                $"# {EmoteService.GetEmoteByName("left_build_icon1")}  `Experience {profile.Experience} `   \n" +
                $"**Badges**: \n# **{badges}**\n" +
                $"**Unlocked Skins**: \n# **{unlockedSkins}**\n" +
                $"Connected At: ` {account.LinkedAtUtc} `\n" +
                $"Service: ` {account.ServiceName} `\n" +
                $"Youtube: ` {BotUser.YouTubeChannel} `\n" +
                $"``` {account.Notes} ```\n");

            var containerb = new ContainerBuilder()
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
              .WithAccentColor(3618621)

              .WithSection(sectionProfile)
              .WithSeparator(SeparatorSpacingSize.Small, true)
              .WithSection(sectionAccount)
             // .WithSeparator(SeparatorSpacingSize.Large, true)
             // .WithSection(sectionRank)
              ;


            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb)
                .WithActionRow([ButtonProfileModule.BuildButton(this)]);

            await ModifyMenuMessageAsync(components: cb.Build());
        }
    }
}
