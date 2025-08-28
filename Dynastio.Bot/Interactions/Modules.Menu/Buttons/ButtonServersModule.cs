using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
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
    /// TEMPLATE: Copy this class to implement a paginated “Servers” menu.
    /// Requires Guild context. Fetches, sorts, paginates, and renders
    /// the list of online servers in a Discord menu.
    /// </summary>
    [RequireContext(ContextType.Guild)]
    public class ButtonServersModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants and ID Formats
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Base prefix for all server-menu button custom IDs.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.servers";

        /// <summary>
        /// Format string for paging and sorting parameters:
        /// {0} = sort, {1} = page number, {2} = items per page, {3} = trigger context.
        /// </summary>
        public const string PagingCustomIdFormat = ":{0}:{1}:{2}:{3}";

        // -----------------------------------------------------------------------------------
        // SECTION: IMenuComponentServiceModule Implementation
        // -----------------------------------------------------------------------------------


        /// <summary>
        /// Builds the “Servers” button for the main menu.
        /// Copy into your new module and customize:
        /// label, emote, style, and initial CustomId.
        /// </summary>
        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] suffixArgs)
        {
            // Example stub; replace values as needed.
            return new ButtonBuilder()
                .WithLabel("Servers")
                .WithEmote(module.EmoteService.GetEmoteByName("left_build_icon"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId(BuildCustomId(
                    ServersSortType.Name,
                    page: 1,
                    pageSize: 20,
                    trigger: "menu"));
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Custom ID Factory
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Builds a fully formatted CustomId for server menu interactions.
        /// </summary>
        /// <param name="sort">Sorting criterion (Name or Region).</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="trigger">Context label or trigger source.</param>
        /// <returns>Composite CustomId string.</returns>
        public static string BuildCustomId(
            ServersSortType sort,
            int page,
            int pageSize,
            string trigger)
            => InteractionIdBase
             + PagingCustomIdFormat.StarIfNullFormat(
                   sort.ToString(),
                   page,
                   pageSize,
                   trigger
               );

        // -----------------------------------------------------------------------------------
        // SECTION: Nested Types
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Defines available sort orders for the servers list.
        /// </summary>
        public enum ServersSortType
        {
            Name,
            Region
        }
        // -----------------------------------------------------------------------------------
        // SECTION: Main Interaction Handler
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Displays a paginated, sortable list of online servers.
        /// Routes clicks on buttons whose CustomId matches
        /// InteractionIdBase + PagingCustomIdFormat.
        /// </summary>
        /// <param name="sort">Sort order (default = Region).</param>
        /// <param name="page">Page index (default = 1).</param>
        /// <param name="pageSize">Items per page (default = 20).</param>
        /// <param name="trigger">Context label (default = "main").</param>
        [ComponentInteraction(InteractionIdBase + ":*:*:*:*")]
        public async Task ShowServersAsync(
            ServersSortType sort = ServersSortType.Region,
            int page = 1,
            int pageSize = 20,
            string trigger = "main")
        {
            // 1️⃣ Acknowledge the interaction immediately
            await DeferAsync();

            // 2️⃣ Retrieve live servers; return not found if empty
            var allServers = Dynastio.OnlineServers;
            if (allServers == null || allServers.Count == 0)
            {
                await ModifyCurrentMessageToNotFound();
                return;
            }

            // 3️⃣ Sort by the requested criterion
            allServers = SortServers(allServers, sort);

            // 4️⃣ Paginate the sorted list
            var visibleServers = allServers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5️⃣ Build a rank lookup dictionary
            var rankLookup = new Dictionary<Server, int>(allServers.Count);
            for (int i = 0; i < allServers.Count; i++)
            {
                // +1 for human-friendly ranking
                rankLookup[allServers[i]] = i + 1;
            }

            // 6️⃣ Render table markdown
            string tableContent = BuildServersTable(visibleServers, rankLookup)
                .ToCodeBlock();

            // 7️⃣ Assemble header section
            var headerSection = new SectionBuilder()
                .WithTextDisplay("## Online Servers")
                .WithTextDisplay("Live server data from Dynast.io servers. Updated in real time.")
                .WithAccessory(new ThumbnailBuilder(
                    EmoteService.GetEmoteByName("left_build_icon").Url,
                    "Dynast.io Bot",
                    false));

            // 8️⃣ Build container with content and separators
            var container = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(headerSection)
                .WithTextDisplay(tableContent)
                .WithSeparator(SeparatorSpacingSize.Large, true);

            // 9️⃣ Add pagination controls with custom step sizes
            var pagingControls = new PaginationControls(
                    EmoteService,
                    InteractionIdBase,
                    allServers.Count,
                    page,
                    pageSize)
            {
                DecreaseStep = 5,
                IncreaseStep = 5
            }
                .Build(sort.ToString());

            container.WithActionRow(pagingControls);

            // 🔟 Update the original menu message
            var components = new ComponentBuilderV2()
                .WithContainer(container);

            await ModifyMenuMessageAsync(components: components.Build());
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method (Optional)
        // -----------------------------------------------------------------------------------



        // -----------------------------------------------------------------------------------
        // SECTION: Helper Methods
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Sorts servers by the chosen metric.
        /// </summary>
        private static List<Server> SortServers(
            List<Server> servers,
            ServersSortType sortType)
            => sortType switch
            {
                ServersSortType.Name => servers
                    .OrderBy(s => s.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                ServersSortType.Region => servers
                    .OrderByDescending(s => s.Region)
                    .ToList(),
                _ => servers
            };

        /// <summary>
        /// Builds a markdown table of servers with rank and metadata.
        /// </summary>
        private static string BuildServersTable(
            List<Server> servers,
            Dictionary<Server, int> rankMap)
        {
            var headers = new[] { "#", "Server", "Players", "Mode", "Map", "Events" };
            Func<Server, object>[] selectors =
            {
                s => rankMap[s],
                s => s.Label.TryRemove(16),
                s => $"[{s.PlayersCount}/{s.ConnectionsLimit}]",
                s => s.GameMode,
                s => s.Map,
                s => $"{s.Events.Count} events"
            };

            return servers.ToFormattedTable(headers, selectors);
        }

        /// <summary>
        /// Sample server list for offline/testing scenarios.
        /// </summary>
        private List<Server> GetSampleServers()
        {
            // Uncomment to use live data in tests:
            // return Dynastio.OnlineServers;

            return new List<Server>
            {
                // Add dummy Server objects here
            };
        }


    }
}
