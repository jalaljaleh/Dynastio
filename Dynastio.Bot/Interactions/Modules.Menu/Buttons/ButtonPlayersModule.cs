using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
using Dynastio.Bot.Interactions.Modules.Menu.Modal;
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
    /// TEMPLATE: we made this class to implement a paginated “Players” menu.
    /// Requires Guild context. Fetches, filters, sorts, paginates, and renders
    /// the list of online players, with optional modal search.
    /// </summary>
    public class ButtonPlayersModule : MenuModulesBase, IMenuComponentRule
    {
        //--------------------------------------------------------------------------------
        // SECTION: Dependency Injection
        //--------------------------------------------------------------------------------

        public DynastioApi Dynastio { get; set; }


        //--------------------------------------------------------------------------------
        // SECTION: Builder Method
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Constructs the “Players” button shown in the main menu.
        /// Adjust label, emote, style, and custom ID as needed.
        /// </summary>
        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] args)
        {
            var id = BuildPagingCustomId(1, 20, "menu");

            return new ButtonBuilder()
                .WithLabel("Players")
                .WithEmote(Emoji.Parse("⚔️"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId(id);
        }

        //--------------------------------------------------------------------------------
        // SECTION: Constants and ID Formats
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Custom ID for the Search Modal interaction.
        /// </summary>
        public const string ModalCustomId = "interactions.menu.modal.players";

        /// <summary>
        /// Base prefix for all button interactions in this module.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.players";

        /// <summary>
        /// Format for IDs that only handle simple paging.
        /// {0}=page, {1}=pageSize, {2}=trigger.
        /// </summary>
        public const string PagingCustomIdFormat = ":{0}:{1}:{2}";

        /// <summary>
        /// Extended format including filters and pagination.
        /// {0}=nickname, {1}=server, {2}=team, {3}=privateServers,
        /// {4}=page, {5}=pageSize, {6}=trigger.
        /// </summary>
        public const string FullCustomIdFormat = ":{0}:{1}:{2}:{3}:{4}:{5}:{6}";

        //--------------------------------------------------------------------------------
        // SECTION: Custom ID Factories
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Builds a simple paging CustomId.
        /// </summary>
        public static string BuildPagingCustomId(int page, int pageSize, string trigger = "")
            => InteractionIdBase
             + PagingCustomIdFormat.StarIfNullFormat(page, pageSize, trigger);

        /// <summary>
        /// Builds a full CustomId with filters + pagination.
        /// </summary>
        public static string BuildFullCustomId(
            string playerNickname = "",
            string server = "",
            string team = "",
            bool privateServers = false,
            int page = 1,
            int pageSize = 20,
            string trigger = "main")
            => InteractionIdBase
             + FullCustomIdFormat.StarIfNullFormat(
                 playerNickname, server, team,
                 privateServers, page, pageSize, trigger
             );

        //--------------------------------------------------------------------------------
        // SECTION: Interaction Handlers
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Receive modal submissions from SearchPlayerModalForm.
        /// Forwards values to main ExecuteAsync.
        /// </summary>
        [ModalInteraction(ModalCustomId)]
        public async Task HandleModalAsync(SearchPlayerModalForm modal)
            => await ExecuteAsync(
                modal.PlayerNickname,
                modal.Server,
                modal.Team,
                privateServers: true,
                trigger: "modal"
            );

        /// <summary>
        /// Handle simple paging button clicks.
        /// Invokes ExecuteAsync with only pagination parameters.
        /// </summary>
        [ComponentInteraction(InteractionIdBase + ":*:*:*")]
        [RequireMessageComponentOwner]
        [RequireContext(ContextType.Guild)]
        public async Task HandlePagingAsync(
            int page = 1,
            int pageSize = 20,
            string trigger = "main")
            => await ExecuteAsync(
                "", "", "", false,
                page, pageSize, trigger
            );

        /// <summary>
        /// Central entry point: fetch, filter, sort, paginate,
        /// and render the players list in the menu message.
        /// </summary>
        [ComponentInteraction(InteractionIdBase + ":*:*:*:*:*:*:*")]
        [RequireMessageComponentOwner]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(
            string playerNickname = "",
            string server = "",
            string team = "",
            bool privateServers = false,
            int page = 1,
            int take = 20,
            string trigger = "main")
        {
            await DeferAsync();  // Acknowledge interaction

            // 1️⃣ Fetch current online players
            var players = Dynastio.OnlinePlayers;
            if (players is null)
            {
                await ReplyWithNotFoundAsync();
                return;
            }

            // 2️⃣ Apply text and privacy filters
            players = players
                .Where(p =>
                    p.IsMatched(!string.IsNullOrWhiteSpace(playerNickname)
                        ? playerNickname : null) &&
                    p.Parent.IsMatched(!string.IsNullOrWhiteSpace(server)
                        ? server : null) &&
                    p.Team.Like(!string.IsNullOrWhiteSpace(team)
                        ? team : null) &&
                    p.Parent.IsPrivate == privateServers
                )
                .ToList();

            // 3️⃣ Sort by default criterion (Score)
            players = SortPlayers(players, ToplistSortType.Score);

            // 4️⃣ Paginate results
            var pagedPlayers = players
                .Skip((page - 1) * take)
                .Take(take)
                .ToList();

            // 5️⃣ Build ranking map for table display
            var rankMap = CreateRankMap(players);

            // 6️⃣ Generate markdown table for visible page
            string tableContent = BuildPlayersTable(pagedPlayers, rankMap)
                .ToCodeBlock();

            // 7️⃣ Assemble menu container
            var listSection = new SectionBuilder()
                .WithTextDisplay("## Top Online Players")
                .WithTextDisplay("Live player data from Dynast.io servers. Updated in real time.")
                .WithAccessory(ButtonSearchPlayersModule.BuildButton(this));

            var listContainer = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(listSection);

            // 7a: Highlight top 3 on first page
            if (page == 1)
            {
                var top3Sections = await BuildTopPlayersSectionsAsync(
                    pagedPlayers.Take(3).ToList()
                );
                foreach (var sec in top3Sections)
                {
                    listContainer.WithSection(sec);
                    listContainer.WithSeparator(SeparatorSpacingSize.Small, true);
                }
            }

            listContainer.WithTextDisplay(tableContent);
            listContainer.WithSeparator(SeparatorSpacingSize.Large, true);

            // 8️⃣ Add pagination controls
            var pagingButtons = new PaginationControls(
                EmoteService,
                InteractionIdBase,
                players.Count,
                page,
                take
            )
            {
                MaxRowsRefreshPage = 20
            }.Build();
            listContainer.WithActionRow(pagingButtons);

            // 9️⃣ Render the updated message components
            var components = new ComponentBuilderV2()
                .WithContainer(listContainer);
            await ModifyMenuMessageAsync(components: components.Build());
        }

        //--------------------------------------------------------------------------------
        // SECTION: Helper Methods
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Sorts players by the given criterion.
        /// </summary>
        private List<Player> SortPlayers(List<Player> players, ToplistSortType sortType)
            => sortType switch
            {
                ToplistSortType.Score => players.OrderByDescending(p => p.Score).ToList(),
                ToplistSortType.Level => players.OrderByDescending(p => p.Level).ToList(),
                ToplistSortType.Nickname => players.OrderBy(p => p.Nickname).ToList(),
                ToplistSortType.Team => players.OrderBy(p => p.Team).ToList(),
                ToplistSortType.ServerName => players.OrderBy(p => p.Parent.Label).ToList(),
                ToplistSortType.Location => players.OrderByDescending(p => p.X * p.Y).ToList(),
                _ => players
            };

        /// <summary>
        /// Creates a map of each player to its zero-based rank index.
        /// </summary>
        private Dictionary<Player, int> CreateRankMap(List<Player> sortedPlayers)
        {
            var map = new Dictionary<Player, int>(sortedPlayers.Count);
            for (int i = 0; i < sortedPlayers.Count; i++)
                map[sortedPlayers[i]] = i;
            return map;
        }

        /// <summary>
        /// Builds a markdown table of the visible players with headers.
        /// </summary>
        private string BuildPlayersTable(
            List<Player> visible,
            Dictionary<Player, int> rankMap)
        {
            var headers = new[] { "#", "server", "score", "level", "team", "nickname" };
            Func<Player, object>[] selectors =
            {
                p => RankIcon(rankMap[p]),
                p => p.Parent.Label.TryRemove(16),
                p => p.Score.ToMetric(),
                p => p.Level.ToMetric(),
                p => p.Team.RemoveLines().TryRemove(6),
                p => p.Nickname.RemoveLines().TryRemove(12)
            };
            return visible.ToFormattedTable(headers, selectors);
        }

        /// <summary>
        /// Builds highlight sections for the top 3 players,
        /// including badges, avatars, and stats.
        /// </summary>
        private async Task<List<SectionBuilder>> BuildTopPlayersSectionsAsync(
            List<Player> topPlayers)
        {
            var sections = new List<SectionBuilder>();
            for (int i = 0; i < topPlayers.Count; i++)
            {
                var p = topPlayers[i];
                var section = new SectionBuilder();
                string displayName = p.Nickname;
                string badges = "";

                if (p.IsAuth)
                {
                    try
                    {
                        var profile = await Dynastio.GetUserProfileAsync(p.Id);
                        badges = string.Join(" ", profile.Badges.Select(a => EmoteService.GetEmote(a)));
                    }
                    catch { /* ignore errors */ }

                    var user = await ResolveDiscordUserAsync(p);
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
                        EmoteService.GetEmote(BadgeType.Premium).Url,
                        "Dynast.io Bot",
                        false
                    ));
                }

                var badgeEmote = i switch
                {
                    0 => EmoteService.GetEmote(BadgeType.CupGold),
                    1 => EmoteService.GetEmote(BadgeType.CupSilver),
                    _ => EmoteService.GetEmote(BadgeType.CupBronze)
                };

                section
                    .WithTextDisplay($"## {badgeEmote} {(i + 1).ToRegularCounter()}. {displayName}")
                    .WithTextDisplay(
                        $"Badges: {badges}\n" +
                        $"Server: ` {p.Parent.Secret} ` Team: `{p.Team}`\n" +
                        $"Level: ` {p.Level} `  Score: ` {p.Score.ToMetric()} `"
                    );

                sections.Add(section);
            }
            return sections;
        }

        /// <summary>
        /// Returns a trophy emoji for top-3 or a regular counter otherwise.
        /// </summary>
        private string RankIcon(int index)
            => index < 3
                ? $"🏆{index + 1}"
                : (index + 1).ToRegularCounter();

        /// <summary>
        /// Resolves a Discord user from a Player object,
        /// either via direct Discord auth or account lookup.
        /// </summary>
        private async Task<IUser> ResolveDiscordUserAsync(Player player)
        {
            if (player.IsDiscordAuth)
            {
                var userId = ulong.Parse(player.Id.Replace("discord:", ""));
                return await Context.Client.GetUserAsync(userId);
            }

            var botUser = await Context.UsersService.GetUserByAccountIdAsync(player.Id);
            return botUser == null
                ? null
                : await Context.Client.GetUserAsync(botUser.Id);
        }

        //--------------------------------------------------------------------------------
        // SECTION: Nested Types
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Sort criteria for the player toplist.
        /// </summary>
        public enum ToplistSortType
        {
            Score,
            Level,
            Nickname,
            Team,
            ServerName,
            Location
        }
    }
}
