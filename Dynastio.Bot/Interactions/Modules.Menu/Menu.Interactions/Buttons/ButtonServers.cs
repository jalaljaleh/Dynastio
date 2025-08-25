using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot;
using Dynastio.Bot.Database;
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

namespace Dynastio.Bot.Interactions.Modules.Menu.Menu.Interactions.Buttons
{
    [RequireContext(ContextType.Guild)]
    public class ButtonServers : MenuModulesBase
    {
        public const string Id = "interactions.buttons.menu.servers";
        public enum ServersSortType
        {
            Name, Region
        }

        /// <summary>
        /// Main interaction handler: displays a paginated leaderboard.
        /// </summary>
        [ComponentInteraction(Id + ":*:*:*:*")]
        public async Task ServersAsync(ServersSortType sort=ServersSortType.Region, int page = 1, int take = 20, string trigger = "main")
        {
            // Acknowledge interaction early
            await DeferAsync();

            // 1. Retrieve or mock your live server list
            var servers = GetSampleServers();
            if (servers == null || servers.Count == 0)
            {
                await NotFound();
                return;
            }

            // 2. Sort once (or change to dynamic sortType)
            servers = SortServers(servers, ServersSortType.Name);

            // 3. Extract only the slice we need for this page
            var pagedServers = servers
                .Skip((page - 1) * take)
                .Take(take)
                .ToList();

            // 4. Build a fast lookup map from Player -> Rank index
            //    This avoids O(n²) calls to List.IndexOf during rendering.
            var rankMap = new Dictionary<Server, int>(servers.Count);
            for (int i = 0; i < servers.Count; i++)
                rankMap[servers[i]] = i;

            string tableContent = BuildServersTable(pagedServers,rankMap)
                                  .ToCodeBlock();

            // 6. Build the main list container
            var listSection = new SectionBuilder()
                .WithTextDisplay("## Online Servers")
                .WithTextDisplay("Live server data from Dynast.io servers. Updated in real time.")
                .WithAccessory(new ThumbnailBuilder(
                    EmoteService.GetEmoteByName("left_build_icon").Url,
                    "Dynast.io Bot",
                    false));

            var listContainer = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(listSection);

            // 7. Combine containers into the component builder
            var components = new ComponentBuilderV2()
                .WithContainer(listContainer);

            listContainer.WithTextDisplay(tableContent);
            listContainer.WithSeparator(SeparatorSpacingSize.Large, true);
            // 8. Add pagination buttons, preserving all state via Custom ID

            // Attach navigation to the last container
            var pagingButtons = new PaginationControls(EmoteService, Id, servers.Count, page, take)
            {
                DecreaseStep = 5,
                IncreaseStep = 5,
            } .Build(sort.ToString());
            listContainer.WithActionRow(pagingButtons);

            // 9. Send the updated components back to Discord
            await ModifyMenuMessageAsync(components: components.Build());
        }

        // ------------------- HELPERS -------------------

        /// <summary>
        /// Sorts the list according to the chosen metric.
        /// </summary>
        private List<Server> SortServers(List<Server> servers, ServersSortType sortType)
        {
            return sortType switch
            {
                ServersSortType.Name => servers.OrderBy(p => p.Label).ToList(),
                ServersSortType.Region => servers.OrderByDescending(p => p.Region).ToList(),
                _ => servers
            };
        }

        /// <summary>
        /// Builds a Markdown table from the visible Server,
        /// </summary>
        private string BuildServersTable(List<Server> visible, Dictionary<Server, int> rankMap)
        {
            // Column headers
            var headers = new[] { "#", "server", "players", "mode", "map", "events" };

            // Selectors for each column
            Func<Server, object>[] selectors = new Func<Server, object>[]
            {
                // Use rankMap instead of IndexOf
                p => rankMap[p],
                p => p.Label.TryRemove(16),
                p => $"[{p.PlayersCount}/{p.ConnectionsLimit}]",
                p => p.GameMode,
                p => p.Map,
                p => p.Events.Count + " events"
            };

            // Leverage your existing ToTable extension
            return visible.ToFormattedTable(headers, selectors);
        }


        /// <summary>
        /// Returns a placeholder list of servers. Replace with live data access.
        /// </summary>
        private List<Server> GetSampleServers()
        {
            //  return Dynastio.OnlineServers;
            return new List<Server>
            {
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                new Server(){ConnectionsLimit = 100,Region ="USA",Events = new(),GameMode ="default" , IsPrivate = false,Label = "frankfurt01",Map = "standard",PlayersCount = 30},
                // Add more server...
            };
        }
    }
}
