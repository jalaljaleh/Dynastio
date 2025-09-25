using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Bot.Utilities;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Drawing;
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
        public const string IdParameterFormat = ":{0}:{1}";

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------

        public static ButtonBuilder BuildButton(MenuModulesBase module, int statType, params string[] args)
        {
            var stat = args.FirstOrDefault().ToString();
            var emote = stat switch
            {
                "kill" => "skull",
                "death" => "guard",
                "gather" => "left_shop_icon1",
                "craft" => "left_craft_icon",
                _ => "not found"
            };
            var btn = new ButtonBuilder()
                .WithLabel("Stats: " + stat)
                .WithEmote(module.EmoteService.GetEmoteByName(emote))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(statType, trigger: stat));
            return btn;
        }
        private static readonly List<string> types = new() { "Kill", "Death", "Gather", "Craft" };
        public static SelectMenuBuilder BuildSelectMenu(MenuModulesBase module)
        {
            var statType = types.Select(a => new SelectMenuOptionBuilder()
                                    .WithLabel(a + " Stat")
                                    .WithDescription($"Your {types}'s stat")
                                    .WithDefault(false)
                                    .WithValue((types.IndexOf(a) + 1).ToString())
                                    .WithEmote(module.EmoteService.GetEmoteByName("left_team_icon")))
                    .ToList();

            var accountsBuilder = new SelectMenuBuilder()
            {
                IsDisabled = false,
                MinValues = 1,
                MaxValues = 1,
                Options = statType,
                CustomId = BuildCustomId(Common.Random.Next(0, 4)),
                Placeholder = "Select Stat Type !"
            };
            return accountsBuilder;
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
        public static string BuildCustomId(int type, string trigger = "")
        {
            // Concatenate base prefix + formatted parameters
            // .StarIfNullFormat ensures safe formatting even if trigger is null/empty
            return InteractionIdBase
                 + IdParameterFormat.StarIfNullFormat(type, trigger);
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
        [ComponentInteraction(InteractionIdBase + ":*:*")]
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireLinkedAccount]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(int statType = 1, string trigger = "")
        {
            await DeferAsync();

            var profile = Context.BotUser.GetDefaultAccount();
            var stat = await profile.GetCachedProfileStatAsync(Context.Dynastio);
            if (stat == null)
            {
                await ReplyWithErrorAsync("Can't find your stat !");
                return;
            }

            // 2) Your original switch now becomes
            //string content = trigger switch
            //{
            //    "kill" => FormatStatRows(stat.Kill),
            //    "gather" => FormatStatRows(stat.Gather),
            //    "death" => FormatStatRows(stat.Death),
            //    "craft" => FormatStatRows(stat.Craft),
            //    _ => "not found"
            //};
            var targetType = (Context.Interaction as SocketMessageComponent)?.Data?.Values?.First() ?? statType.ToString();
            statType = int.Parse(targetType);

            string content = statType switch
            {
                1 => FormatStatRows(stat.Kill),
                2 => FormatStatRows(stat.Gather),
                3 => FormatStatRows(stat.Death),
                4 => FormatStatRows(stat.Craft),
                _ => "not found"
            };
            string contentType = types[statType - 1];

            if (content.Length > 3800)
                content = content.Substring(0, 3800);

            var section = new SectionBuilder()
                .WithTextDisplay($"# {contentType} Stats" +
                $"\nYou are logined as **{profile.DisplayName}**, here you can see your game stats sorted and filtered by kills, death, geathers and craft !")
                .WithAccessory(new ThumbnailBuilder(User.TryGetAvatarUrl()));

            var containerb = new ContainerBuilder()
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
            //  .WithAccentColor(Color.Green)
              .WithSection(section)
              .WithActionRow([BuildSelectMenu(this)])
              .WithTextDisplay( content);
            //.WithActionRow([
            //    BuildButton(this,"kill"),
            //    BuildButton(this,"gather"),
            //    BuildButton(this,"death"),
            //    BuildButton(this,"craft")]);

            var page = new PaginationControls(EmoteService, InteractionIdBase, 4, statType, 1)
                .WithRefreshButton(false)
                .WithSizeControlButtons(false)
                .Build();

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb)
                .WithActionRow(page);

            await ReplyOrModifyAsync(components: cb.Build());

        }
        string FormatStatRows<T>(Dictionary<T, int> data) where T : struct, Enum
        {
            return string.Concat(
                data
                .OrderByDescending(x => x.Value)
                .Select((x, idx) => new
                {
                    Text = EmoteService.GetEmote<T>(x.Key).ToString() + $"x{x.Value.ToMetric()}",
                    Group = idx / 6
                })
                .GroupBy(x => x.Group)
                .Select(g => "\n# " + string.Join(" , ", g.Select(e => e.Text)))
            );
        }
    }
}
