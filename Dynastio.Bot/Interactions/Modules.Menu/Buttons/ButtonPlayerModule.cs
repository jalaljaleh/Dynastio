using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Services;
using Dynastio.Extenstions;
using Dynastio.Net;
using System.Numerics;
using System.Runtime.ExceptionServices;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonPlayerModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public DynastioApi Dynastio { get; set; }
        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.player";

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
                .WithLabel("Player")
                .WithEmote(module.EmoteService.GetEmoteByName("tab_profile_icon_active"))
                .WithStyle(ButtonStyle.Success)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: args.FirstOrDefault()));
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

        public static SelectMenuBuilder BuildSelectMenu(MenuModulesBase module, params Player[] players)
        {
            if (players.Length < 1) return null;
            var players_ = players.Take(24).Select(a => new SelectMenuOptionBuilder()
                                .WithLabel("Player " + a.Nickname)
                                .WithDescription("Server " +a.Parent.Label)
                                .WithDefault(false)
                                .WithValue(a.InternalId.ToString())
                                .WithEmote(module.EmoteService.GetEmoteByName("left_team_icon")))
                .ToList();

            var accountsBuilder = new SelectMenuBuilder()
            {
                IsDisabled = false,
                MinValues = 1,
                MaxValues = 1,
                Options = players_,
                CustomId = BuildCustomId(),
            };
            return accountsBuilder;
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

            var players = Dynastio.OnlinePlayers ?? Dynastio.OnlineTopPlayers;

            var data = (Context.Interaction as SocketMessageComponent)?.Data?.Values?.First() ?? trigger;

            var player = players.FirstOrDefault(a => a.InternalId.ToString() == data);
            if (player is null)
            {
                await ReplyWithNotFoundAsync();
                return;
            }

            var section = new SectionBuilder();
            string displayName = player.Nickname;
            string badges = "` no any badge `";
            int coins = -1;
            int profileLevel = -1;
            int exp = 0;
            int expMax = 10;

            if (player.IsAuth)
            {
                try
                {
                    var profile = await Dynastio.GetUserProfileAsync(player.Id);
                    badges = string.Join(" ", profile.Badges.Select(a => EmoteService.GetEmoteTag(a)));
                    coins = profile.Coins;
                    profileLevel = profile.Level;
                    expMax = (int)profile.GetExperienceMax();


                }
                catch { /* ignore errors */ }

                var user = await DiscordResolver.ResolveDiscordUserAsync(player, Context.Client, UsersService);
                if (user != null)
                {
                    displayName = user.Mention;
                    section.WithAccessory(new ThumbnailBuilder(
                        user.TryGetAvatarUrl(),
                        user.Username,
                        false
                    ));
                }
            }
            else
            {
                section.WithAccessory(new ThumbnailBuilder(
                    Context.Guild.IconUrl,
                    "Dynast.io Bot",
                    false
                ));
            }

            section
                .WithTextDisplay(
                $"# {EmoteService.GetEmoteByName("left_team_icon")} Player  ` {displayName} `" +
                $"\n The player ` {displayName} ` is now playing on **{player.Parent.Label}** Click to **[-> Join]({player.Parent.DirectLink})** !" +
                  $"\n## {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")} Game Level: ` {player.Level} `\t\t{EmoteService.GetEmoteByName("mainmenu_level_shield_premium")} Global Level: ` {profileLevel} `" +
                  $"\n### {EmoteService.GetEmoteByName("mainmenu_level_shield_premium")} Experience: {EmoteService.BuildProgressBar(10, exp, expMax)}" +
                  $"\n### {EmoteService.GetEmoteByName("coins3")} Coins: ` {coins.ToMetric()} `" +
                  $"\n### {(string.IsNullOrEmpty(badges) ? "No badge" : " " + badges)}" +
                  ""
                );

            var team = Dynastio.Teams?.FirstOrDefault(a => a.Name.Equals(player.Team)) ?? new Team() { Players = new() };

            var container = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
                .WithAccentColor(Color.DarkGreen)
                .WithTextDisplay("Dynast.io Player")
                .WithSeparator(SeparatorSpacingSize.Small, true)
                .WithSection(section)

                .WithSeparator(SeparatorSpacingSize.Small, true)
                .WithTextDisplay($"## {EmoteService.GetEmoteByName("left_team_icon")} Player\n" + player.ToTable().ToCodeBlock())

                .WithSeparator(SeparatorSpacingSize.Small, true)
                .WithTextDisplay($"## {EmoteService.GetEmoteByName("left_trade_icon")} Teammates\n" + team.Players.ToTable().ToCodeBlock())

                .WithSeparator(SeparatorSpacingSize.Small, true)
                .WithTextDisplay($"## {EmoteService.GetEmoteByName("left_build_icon")} Server **[--> Join]({player.Parent.DirectLink})**\n" + player.Parent.ToTable().ToCodeBlock());

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(container)
           //     .WithActionRow([ButtonPlayerModule.BuildButton(this, trigger)])
            ;
            await ReplyOrModifyAsync(components: cb.Build());

        }
    }
}
