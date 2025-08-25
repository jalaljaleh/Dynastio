using Discord;
using Dynastio.Bot.Services;
using System;

namespace Dynastio.Bot.Utilities
{
    /// <summary>
    /// Builds a standard Discord ActionRow containing pagination buttons (Back/Next)
    /// and optional controls for changing the number of rows per page or refreshing the data.
    /// </summary>
    internal class PaginationControls
    {
        // ===== PRIVATE STATE =====
        private readonly int _currentPage;      // Current page number (1-based)
        private readonly int _totalItemCount;   // Total number of items in the dataset
        private readonly int _rowsPerPage;      // How many rows are shown per page
        private readonly string _baseCustomId;  // Base ID prefix for buttons
        private readonly EmoteService _emoteService; // Service used to get emotes/icons

        /// <summary>
        /// Creates a new pagination control builder.
        /// </summary>
        /// <param name="emoteService">EmoteService instance for fetching button icons.</param>
        /// <param name="currentPage">Current page (minimum 1).</param>
        /// <param name="rowsPerPage">Number of rows per page (minimum 1).</param>
        /// <param name="totalItems">Total number of items in the dataset (minimum 0).</param>
        /// <param name="baseCustomId">Base custom ID used as the prefix for all button IDs.</param>
        public PaginationControls(EmoteService emoteService, string baseCustomId, int totalItems, int currentPage = 1, int rowsPerPage = 15)
        {
            _emoteService = emoteService ?? throw new ArgumentNullException(nameof(emoteService));
            _currentPage = Math.Max(1, currentPage);
            _rowsPerPage = Math.Max(1, rowsPerPage);
            _totalItemCount = Math.Max(0, totalItems);
            _baseCustomId = baseCustomId ?? throw new ArgumentNullException(nameof(baseCustomId));
        }

        // ===== CONFIGURATION FLAGS =====

        /// <summary>If true, Back/Next page buttons will be shown. Default: True</summary>
        public bool ShowPageNavigation { get; set; } = true;

        /// <summary>If true, a Refresh button will be shown. Default: True</summary>
        public bool ShowRefreshButton { get; set; } = true;

        /// <summary>If true, buttons to change the number of rows per page will be shown. Default: True</summary>
        public bool ShowRowSizeControls { get; set; } = true;

        // ===== ROW SIZE CONTROL SETTINGS =====

        /// <summary>Number of rows to add when clicking the increase (+) button. Default: 10</summary>
        public int IncreaseStep { get; set; } = 10;

        /// <summary>Number of rows to remove when clicking the decrease (-) button. Default: 10</summary>
        public int DecreaseStep { get; set; } = 10;

        /// <summary>Minimum allowed rows per page. Default: 1</summary>
        public int MinRowsPerPage { get; set; } = 1;

        /// <summary>Maximum allowed rows per page. Default: 30</summary>
        public int MaxRowsPerPage { get; set; } = 30;

        /// <summary>Maximum allowed rows per page when refresh. <param name="MaxRowsRefreshPage"> Default = 0 disabled</summary>
        public int MaxRowsRefreshPage { get; set; } = 0;
        // ===== MAIN BUILDER =====

        /// <summary>
        /// Builds an ActionRowBuilder containing all enabled pagination controls.
        /// </summary>
        /// <param name="extraPrefix">Optional extra payload to insert before the paging data in the custom ID.</param>
        /// <param name="extraSuffix">Optional extra payload to insert after the paging data in the custom ID.</param>
        public ActionRowBuilder Build(string extraPrefix = null, string extraSuffix = null)
        {
            // Local function to build the full CustomId string with extra data
            string BuildCustomId(string pagingData, string buttonName)
            {
                // Collect all parts, skipping null or empty ones
                var parts = new[]
                {
                            _baseCustomId,
                            extraPrefix,     // Only included if not null/empty
                            pagingData,
                            extraSuffix      // Only included if not null/empty
                            };

                // Join only the non-null/empty parts with ":"
                return string.Join(":", parts.Where(p => !string.IsNullOrEmpty(p))) + $":paginationcontrols_button_{buttonName}";
            }

            var row = new ActionRowBuilder();

            // BACK BUTTON
            if (ShowPageNavigation)
            {
                row.AddComponent(CreateButton(
                    label: "Previous",
                    customId: BuildCustomId($"{_currentPage - 1}:{_rowsPerPage}", "back"),
                    style: ButtonStyle.Primary,
                    emoteName: "travolatorleft",
                    disabled: _currentPage <= 1
                ));
            }

            // DECREASE ROW COUNT BUTTON
            if (ShowRowSizeControls && DecreaseStep > 0)
            {
                row.AddComponent(CreateButton(
                    label: $"- {DecreaseStep}",
                    customId: BuildCustomId($"{_currentPage}:{_rowsPerPage - DecreaseStep}", "decrease"),
                    style: ButtonStyle.Secondary,
                    emoteName: "zoomout",
                    disabled: _rowsPerPage - DecreaseStep < MinRowsPerPage
                ));
            }

            // REFRESH BUTTON
            if (ShowRefreshButton)
            {
                row.AddComponent(CreateButton(
                    label: "Refresh",
                    customId: BuildCustomId($"{1}:{(MaxRowsRefreshPage == 0 ? _rowsPerPage : MaxRowsRefreshPage)}", "refresh"),
                    style: ButtonStyle.Success,
                    emoteName: "portal"
                ));
            }

            // INCREASE ROW COUNT BUTTON
            if (ShowRowSizeControls && IncreaseStep > 0)
            {
                row.AddComponent(CreateButton(
                    label: $"+ {IncreaseStep}",
                    customId: BuildCustomId($"{_currentPage}:{_rowsPerPage + IncreaseStep}", "increase"),
                    style: ButtonStyle.Secondary,
                    emoteName: "zoomin",
                    disabled: _rowsPerPage + IncreaseStep > MaxRowsPerPage
                ));
            }

            // NEXT BUTTON
            if (ShowPageNavigation)
            {
                row.AddComponent(CreateButton(
                    label: "Next",
                    customId: BuildCustomId($"{_currentPage + 1}:{_rowsPerPage}", "next"),
                    style: ButtonStyle.Primary,
                    emoteName: "travolatorright",
                    disabled: _currentPage * _rowsPerPage >= _totalItemCount
                ));
            }

            return row;
        }

        // ===== HELPER =====

        /// <summary>
        /// Creates a styled Discord button with label, custom ID, style, emote, and disabled state.
        /// </summary>
        private ButtonBuilder CreateButton(string label, string customId, ButtonStyle style, string emoteName, bool disabled = false)
        {
            return new ButtonBuilder()
                .WithLabel(label)
                .WithCustomId(customId)
                .WithStyle(style)
                .WithEmote(_emoteService.GetEmoteByName(emoteName))
                .WithDisabled(disabled);
        }
    }
}
