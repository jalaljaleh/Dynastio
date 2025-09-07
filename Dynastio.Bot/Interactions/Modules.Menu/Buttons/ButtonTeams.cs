using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Bot.Utilities;
using Dynastio.Extenstions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class to implement a paginated “Teams” menu button.
    /// Requires Guild context. Fetches, sorts, paginates, and renders
    /// the list of teams in a Discord menu interaction.
    /// </summary>
    public class ButtonTeamsModule : MenuModulesBase, IMenuComponentRule
    {
        //--------------------------------------------------------------------------------
        // SECTION: Dependency Injection
        //--------------------------------------------------------------------------------
        public DynastioApi Dynastio { get; set; }

        /// <summary>
        /// Prefix for all CustomIds in this module.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.teams";

        /// <summary>
        /// Format string appended to the base ID:
        /// {0} = page number, {1} = items per page, {2} = trigger/context string.
        /// </summary>
        public const string IdParameterFormat = ":{0}:{1}:{2}";

      

        // -----------------------------------------------------------------------------------
        // SECTION: Custom ID Factory
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Builds a CustomId for button clicks, embedding paging + context.
        /// </summary>
        /// <param name="page">Current page number (default=1).</param>
        /// <param name="take">Items per page (default=10).</param>
        /// <param name="trigger">Optional context label.</param>
        /// <returns>Fully formatted CustomId.</returns>
        public static string BuildCustomId(int page = 1, int take = 10, string trigger = "")
            => InteractionIdBase
             + IdParameterFormat.StarIfNullFormat(page, take, trigger);

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Constructs the “Teams” ButtonBuilder for the menu UI.
        /// Copy-and-paste into your new module and customize label, style, emote, and ID.
        /// </summary>
        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] suffixArgs)
        {
            return new ButtonBuilder()
                .WithLabel(module["buttons.interactions.menu.teams.label"])
                .WithEmote(new Emoji("🎪"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId(BuildCustomId(1, 10, "menu"));
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Main Interaction Handler
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Invoked when a Teams button is clicked. Handles fetching,
        /// sorting, paginating, and rendering the teams list.
        /// </summary>
        /// <param name="page">Page index (default=1).</param>
        /// <param name="pageSize">Items per page (default=10).</param>
        /// <param name="trigger">Context string (default="").</param>
        [ComponentInteraction(InteractionIdBase + ":*:*:*")]
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireContext(ContextType.Guild)]
        public async Task HandleTeamsButtonAsync(int page = 1, int pageSize = 10, string trigger = "")
        {
            // 1️⃣ Acknowledge interaction to prevent timeout
            await DeferAsync();

            // 2️⃣ Retrieve all teams (live API or cache)
            var _allTeams = await Dynastio.GetTeamsAsync().TryAsync();
            if (_allTeams.isSuccessful is false || _allTeams.result == null || _allTeams.result.Count == 0)
            {
                await ReplyWithNotFoundAsync();
                return;
            }
            var allTeams = _allTeams.result;

            // 3️⃣ Sort teams (currently by Name; extendable)
            allTeams = SortTeams(allTeams, TeamsSortOrder.Name);

            // 4️⃣ Paginate the sorted list
            var pagedTeams = allTeams
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5️⃣ Build ranking lookup for quick index access
            var rankLookup = new Dictionary<Team, int>(allTeams.Count);
            for (int i = 0; i < allTeams.Count; i++)
                rankLookup[allTeams[i]] = i + 1; // human-friendly 1-based rank

            // 6️⃣ Render teams as a markdown table
            var tableContent = BuildTeamsTable(pagedTeams, rankLookup)
                .ToCodeBlock();

            // 7️⃣ Assemble header section for the menu
            var section = new SectionBuilder()
                .WithTextDisplay("## Online Teams")
                .WithTextDisplay("Live team data from Dynast.io Teams. Updated in real time.")
                .WithAccessory(new ThumbnailBuilder(
                    EmoteService.GetEmoteByName("mainmenu_level_shield_premium").Url,
                    "Dynast.io Bot",
                    false));

            // 8️⃣ Build container with table and separator
            var container = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(section)
                .WithTextDisplay(tableContent)
                .WithSeparator(SeparatorSpacingSize.Large, true);

            // 9️⃣ Add pagination controls with custom steps
            var pagingControls = new PaginationControls(
                EmoteService,
                InteractionIdBase,
                allTeams.Count,
                page,
                pageSize)
            {
                DecreaseStep = 5,
                IncreaseStep = 5
            }.Build();

            container.WithActionRow(pagingControls);

            // 🔟 Send updated components back to Discord
            var components = new ComponentBuilderV2()
                .WithContainer(container);

            await ModifyMenuMessageAsync(components: components.Build());
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Helper Methods
        // -----------------------------------------------------------------------------------


        /// <summary>
        /// Sorts the teams list according to the specified order.
        /// </summary>
        private static List<Team> SortTeams(List<Team> teams, TeamsSortOrder order)
            => order switch
            {
                TeamsSortOrder.Name => teams
                    .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                _ => teams
            };


        /// <summary>
        /// Builds a markdown table of visible teams with headers and values.
        /// </summary>
        private static string BuildTeamsTable(
            List<Team> visibleTeams,
            Dictionary<Team, int> rankMap)
        {
            var headers = new[] { "#", "Team", "Members", "Server", "Mode" };
            Func<Team, object>[] selectors =
            {
                team => rankMap[team],
                team => team.Name.TryRemove(16),
                team => $"[{team.MembersCount}/100]",
                team => team.Server.Label,
                team => team.Server.GameMode
            };

            return visibleTeams.ToFormattedTable(headers, selectors);
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Nested Types
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Defines possible sort orders for the teams list.
        /// </summary>
        public enum TeamsSortOrder
        {
            Name
        }
    }
}
