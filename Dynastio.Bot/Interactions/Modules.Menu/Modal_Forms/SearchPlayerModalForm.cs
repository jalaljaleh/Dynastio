using Discord;
using Discord.Interactions;

namespace Dynastio.Bot.Interactions.Modules.Menu.Modal
{
    /// <summary>
    /// Modal dialog for entering player search filters:
    /// nickname, team name, and server name.
    /// Implements IModal so Discord.NET can render it automatically.
    /// </summary>
    public sealed class SearchPlayerModalForm : IModal
    {
        /// <summary>
        /// The title shown at the top of the modal.
        /// </summary>
        public string Title => "Search Players";

        // ──────────────── Nickname Input ────────────────

        /// <summary>
        /// Filter by player nickname (optional, max 16 characters).
        /// Leave blank to disable this filter.
        /// </summary>
        [InputLabel("Player Nickname")]
        [RequiredInput(false)]
        [ModalTextInput(
            customId: "nickname",
            style: TextInputStyle.Short,
            placeholder: "Enter nickname (max 16 chars)",
            minLength: 0,
            maxLength: 16)]
        public string PlayerNickname { get; set; } = string.Empty;

        // ──────────────── Team Input ────────────────

        /// <summary>
        /// Filter by team name (optional, max 16 characters).
        /// Leave blank to disable this filter.
        /// </summary>
        [InputLabel("Team")]
        [RequiredInput(false)]
        [ModalTextInput(
            customId: "team",
            style: TextInputStyle.Short,
            placeholder: "Enter team name (max 16 chars)",
            minLength: 0,
            maxLength: 16)]
        public string Team { get; set; } = string.Empty;

        // ──────────────── Server Input ────────────────

        /// <summary>
        /// Filter by server name (optional, max 16 characters).
        /// Leave blank to disable this filter.
        /// </summary>
        [InputLabel("Server")]
        [RequiredInput(false)]
        [ModalTextInput(
            customId: "server",
            style: TextInputStyle.Short,
            placeholder: "Enter server name (max 16 chars)",
            minLength: 0,
            maxLength: 16)]
        public string Server { get; set; } = string.Empty;

        // ──────────────── Helper Properties & Methods ────────────────

        /// <summary>
        /// Returns true if at least one filter field has a nonempty value.
        /// </summary>
        public bool HasAnyFilter =>
            !string.IsNullOrWhiteSpace(PlayerNickname) ||
            !string.IsNullOrWhiteSpace(Team) ||
            !string.IsNullOrWhiteSpace(Server);

        /// <summary>
        /// Constructs a typed filter object for use in your query logic.
        /// Trims whitespace and converts empty strings to null.
        /// </summary>
        public PlayerSearchCriteria ToCriteria() =>
            new PlayerSearchCriteria(
                nickname: Normalize(PlayerNickname),
                team: Normalize(Team),
                server: Normalize(Server)
            );

        /// <summary>
        /// Trims the input and returns null if it’s empty or whitespace.
        /// </summary>
        private static string Normalize(string input) =>
            string.IsNullOrWhiteSpace(input)
                ? null
                : input.Trim();
    }

    /// <summary>
    /// Represents the set of search criteria collected from the modal.
    /// </summary>
    public sealed class PlayerSearchCriteria
    {
        public string Nickname { get; }
        public string Team { get; }
        public string Server { get; }

        public PlayerSearchCriteria(string nickname, string team, string server)
        {
            Nickname = nickname;
            Team = team;
            Server = server;
        }
    }
}
