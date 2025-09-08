using Discord;
using Discord.Interactions;
using Dynastio.Bot.Extensions;
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
    public class ButtonProfileModule : MenuModulesBase, IMenuComponentRule
    {
        //--------------------------------------------------------------------------------
        // SECTION: Dependency Injection
        //--------------------------------------------------------------------------------
        public DynastioApi Dynastio { get; set; }

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

                .WithLabel(module["buttons.interactions.menu.profile.label"])

                .WithEmote(module.EmoteService.GetEmoteByName("left_team_icon"))

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
        [RequireLinkedAccount]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(string trigger = null)
        {
            await DeferAsync();

            var discordUser = Context.User;
            var botUser = Context.BotUser;
            var account = Context.BotUser.GetDefaultAccount();
            var guildProfile = BotUser.GetOrCreateGuildProfile(Guild.Id);
    
            var profile = await account.GetCachedProfileCardAsync(Dynastio);

            /// test
            //profile.UnlockedSkins.AddRange([SkinType.Ninja, SkinType.Snowman, SkinType.Anime, SkinType.Girl]);
            //profile.Badges.AddRange([BadgeType.Administrator, BadgeType.CupBronze, BadgeType.Monthly]);


            var container = new ContainerBuilder()
            .WithAccentColor(Color.Green)
            .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
            .WithTextDisplay($" You logined as **{account.DisplayName}**, Peek into your Dynast.io legacy — see your linked account, level, score, badges, and more. Every survivor has a story… this is yours.")
            .WithSeparator(SeparatorSpacingSize.Large, true);

            string sectionAccountContent = 
                $"# :crossed_swords: {account.DisplayName}\n" +
                $"\n\n" +
                $"# {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")}  Level {profile.Profile.Level} \t{EmoteService.GetEmoteByName("coins3")} Coins {profile.Profile.Coins.ToMetric()} \n" +
                $"## {EmoteService.GetEmoteByName("left_build_icon1")} ` {profile.Profile.Experience} Experience ` {EmoteService.GetEmoteByName("sign")} `{ profile.Profile.GetRequireExperienceForNewLevel()} to levelup ` \n" +
                $"### {EmoteService.GetEmoteByName("tab_profile_icon_active")} Last playing {profile.Profile.LastActiveAt.ToDiscordTimestamp()} in **{profile.Profile.LatestServer}**" +
                "";

            var sectionAccount = new SectionBuilder()
                 .WithAccessory(new ThumbnailBuilder(User.TryGetAvatarUrl()))
              // .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("privatechest").Url))
              .WithTextDisplay(sectionAccountContent);

            container
             .WithSection(sectionAccount)
             .WithSeparator(SeparatorSpacingSize.Small, true);

            // ---------------------   Badge  ---------------------------------------------------------------------


            string badges = string.Join("", profile.Profile.Badges.Select(a => EmoteService.GetEmote(a)));

            var sectionBadges = new SectionBuilder()
             .WithAccessory(ButtonSyncBadgesModule.BuildButton(this))
             .WithTextDisplay($"{(string.IsNullOrEmpty(badges) ? "No badge" : "# " + badges)}");

            container
             .WithSection(sectionBadges);



            // ------------------------------------------------------------------------------------------

            string unlockedSkins = string.Join("", profile.Profile.UnlockedSkins.Select(a => EmoteService.GetEmoteByName("skin_" + a)));

            var sectionSkins = new SectionBuilder()
           .WithAccessory(ButtonCloseModule.BuildButton(this))
            .WithTextDisplay($"**Unlocked Skins**: {(string.IsNullOrEmpty(unlockedSkins) ? "no skin" : "\n# " + unlockedSkins)}");


            var sectionExtra = new SectionBuilder()
            .WithAccessory(ButtonCloseModule.BuildButton(this))
             .WithTextDisplay(
             $"Service: ` {account.ServiceName} `\n" +
             $"Connected At: ` {account.LinkedAtUtc} `\n" +
             $"Youtube: ` {BotUser.YouTubeChannel ?? "not linked"} `\n" +
             $"``` {account.Notes} ```\n");


            container
              .WithSection(sectionSkins)
              .WithSeparator(SeparatorSpacingSize.Large, true)
              .WithSection(sectionExtra)
              .WithActionRow([ButtonCloseModule.BuildButton(this), ButtonCloseModule.BuildButton(this)]);

            // .WithSeparator(SeparatorSpacingSize.Large, true)
            // .WithSection(sectionRank)
            ;


            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(container)

            //   .WithActionRow([ButtonCloseModule.BuildButton(this)]);
            //    .WithActionRow([ButtonProfileModule.BuildButton(this)]);
            ;
            await ReplyOrModifyAsync(components: cb.Build());
        }
    }
}
