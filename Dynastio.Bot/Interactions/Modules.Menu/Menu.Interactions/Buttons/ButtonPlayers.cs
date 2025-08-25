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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Menu.Interactions.Buttons
{
    [RequireContext(ContextType.Guild)]
    public class ButtonPlayers : MenuModulesBase
    {
        public const string Id = "interactions.buttons.menu.players";
        public enum ToplistSortType
        {
            Score, Level, Nickname, Team, ServerName, Location
        }

        /// <summary>
        /// Main interaction handler: displays a paginated leaderboard.
        /// </summary>
        [ComponentInteraction(Id + ":*:*:*")]
        public async Task PlayersAsync(int page = 1, int take = 20,string trigger = "main")
        {
            // Acknowledge interaction early
            await DeferAsync();

            // 1. Retrieve or mock your live player list
            var players = GetSamplePlayers();
            if (players == null || players.Count == 0)
            {
                await NotFound();
                return;
            }

            // 2. Sort once by Score (or change to dynamic sortType)
            players = SortPlayers(players, ToplistSortType.Score);

            // 3. Extract only the slice we need for this page
            var pagedPlayers = players
                .Skip((page - 1) * take)
                .Take(take)
                .ToList();

            // 4. Build a fast lookup map from Player -> Rank index
            //    This avoids O(n²) calls to List.IndexOf during rendering.
            var rankMap = new Dictionary<Player, int>(players.Count);
            for (int i = 0; i < players.Count; i++)
                rankMap[players[i]] = i;

            // 5. Build the Markdown table using the cached rankMap
            string tableContent = BuildPlayersTable(pagedPlayers, rankMap)
                                  .ToCodeBlock();

            // 6. Build the main list container
            var listSection = new SectionBuilder()
                .WithTextDisplay("## Top Online Players")
                .WithTextDisplay("Live player data from Dynast.io servers. Updated in real time.")
                .WithAccessory(new ThumbnailBuilder(
                    EmoteService.GetEmote(EntityType.Heartstone).Url,
                    "Dynast.io Bot",
                    false));

            var listContainer = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(listSection);

            // 7. Combine containers into the component builder
            var components = new ComponentBuilderV2()
                .WithContainer(listContainer);

            // Only show the top-3 highlight on the first page
            if (page == 3)
            {
                var top3 = pagedPlayers.Take(1).ToList();
                var top3Sections = await BuildTopPlayersContainerAsync(top3);
                foreach (var s in top3Sections)
                {
                    listContainer.WithSection(s);
                    listContainer.WithSeparator(SeparatorSpacingSize.Small, true);
                }
            }
            listContainer.WithTextDisplay(tableContent);
            listContainer.WithSeparator(SeparatorSpacingSize.Large, true);
            // 8. Add pagination buttons, preserving all state via Custom ID

            // Attach navigation to the last container
            var pagingButtons = new PaginationControls(EmoteService,Id,players.Count, page, take)
            {
                MaxRowsRefreshPage = 20
            }
                .Build();
            listContainer.WithActionRow(pagingButtons);

            // 9. Send the updated components back to Discord
            await ModifyMenuMessageAsync(components: components.Build());
        }

        // ------------------- HELPERS -------------------

        /// <summary>
        /// Sorts the list according to the chosen metric.
        /// </summary>
        private List<Player> SortPlayers(List<Player> players, ToplistSortType sortType)
        {
            return sortType switch
            {
                ToplistSortType.Score => players.OrderByDescending(p => p.Score).ToList(),
                ToplistSortType.Level => players.OrderByDescending(p => p.Level).ToList(),
                ToplistSortType.Nickname => players.OrderBy(p => p.Nickname).ToList(),
                ToplistSortType.Team => players.OrderBy(p => p.Team).ToList(),
                ToplistSortType.ServerName => players.OrderBy(p => p.Parent.Label).ToList(),
                ToplistSortType.Location => players.OrderByDescending(p => p.X * p.Y).ToList(),
                _ => players
            };
        }

        /// <summary>
        /// Builds a Markdown table from the visible players,
        /// using a pre-computed rankMap for O(1) rank lookups.
        /// </summary>
        private string BuildPlayersTable(List<Player> visible, Dictionary<Player, int> rankMap)
        {
            // Column headers
            var headers = new[] { "#", "server", "score", "level", "team", "nickname" };

            // Selectors for each column
            Func<Player, object>[] selectors = new Func<Player, object>[]
            {
                // Use rankMap instead of IndexOf
                p => RankIcon(rankMap[p]),
                p => p.Parent.Label.TryRemove(16),
                p => p.Score.ToMetric(),
                p => p.Level.ToMetric(),
                p => p.Team.RemoveLines().TryRemove(6),
                p => p.Nickname.RemoveLines().TryRemove(12)
            };

            // Leverage your existing ToTable extension
            return visible.ToFormattedTable(headers, selectors);
        }

        /// <summary>
        /// Builds a container highlighting the top 3 players with badges.
        /// </summary>
        private async Task<List<SectionBuilder>> BuildTopPlayersContainerAsync(List<Player> topPlayers)
        {
            List<SectionBuilder> sections = new();

            for (int i = 0; i < topPlayers.Count; i++)
            {
                var player = topPlayers[i];
                var section = new SectionBuilder();
                string displayName = player.Nickname;
                string badges = "";
                // If the player is authenticated, show their Discord avatar & mention
                if (player.IsAuth)
                {
                    try
                    {
                        var profile = await Dynastio.GetUserProfileAsync(player.Id);
                        badges = string.Join(" ", profile.Badges.Select(a => EmoteService.GetEmote(a)));
                    }
                    catch
                    {

                    }

                    var user = await ResolveDiscordUserAsync(player);
                    if (user != null)
                    {
                        displayName = user.Mention;
                        section.WithAccessory(new ThumbnailBuilder(
                            user.TryGetAvatarUrl(), user.Username, false));
                    }
                }
                else
                {
                    // Fallback badge for guest players
                    section.WithAccessory(new ThumbnailBuilder(
                        EmoteService.GetEmote(BadgeType.Premium).Url,
                        "Dynast.io Bot",
                        false));
                }

                // Pick gold/silver/bronze
                var badge = i switch
                {
                    0 => EmoteService.GetEmote(BadgeType.CupGold),
                    1 => EmoteService.GetEmote(BadgeType.CupSilver),
                    _ => EmoteService.GetEmote(BadgeType.CupBronze),
                };

                // Display rank, name, and stats
                section.WithTextDisplay($"## {badge} {(i + 1).ToRegularCounter()}. {displayName}")
                       .WithTextDisplay(
                           $"Badges: {badges}\n" +
                           $"Server: ` {player.Parent.Secret} ` Team: `{player.Team}`\n" +
                           $"Level: ` {player.Level} `  Score: ` {player.Score.ToMetric()} `"
                       );

                sections.Add(section);
            }

            return sections;
        }

        /// <summary>
        /// Converts a zero-based rank into an emoji or counter string.
        /// </summary>
        private string RankIcon(int index) =>
            index < 3
                ? $"🏆{index + 1}"
                : (index + 1).ToRegularCounter();

        /// <summary>
        /// Resolves a Player into a Discord IUser for mentions & avatars.
        /// </summary>
        private async Task<IUser> ResolveDiscordUserAsync(Player player)
        {
            if (player.IsDiscordAuth)
            {
                // Strip "discord:" prefix and fetch directly
                var userId = ulong.Parse(player.Id.Replace("discord:", ""));
                return await Context.Client.GetUserAsync(userId);
            }

            // Fallback: look up via your UsersService
            var botUser = await Context.UsersService.GetUserByAccountIdAsync(player.Id);
            if (botUser is null) return null;
            return await Context.Client.GetUserAsync(botUser.Id);
        }


        /// <summary>
        /// Returns a placeholder list of players. Replace with live data access.
        /// </summary>
        private List<Player> GetSamplePlayers()
        {
            //  return Dynastio.OnlinePlayers;
            return new List<Player>
            {
                new Player {
                    Id = "google:101593599263708684778",
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 9999999999,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                }, new Player {
                    Nickname = "Jaleh",
                    Level    = 10,
                    Score    = 10000,
                    Team     = "Aliens",
                    Parent   = new Server { Label = "Frankfurt" }
                },
                new Player {
                    Nickname = "Sara",
                    Level    = 20,
                    Score    = 20000,
                    Team     = "Berliners",
                    Parent   = new Server { Label = "Berlin" }
                },
                // Add more players...
            };
        }
    }
}
