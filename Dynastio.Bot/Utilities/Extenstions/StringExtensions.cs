using Amazon.Runtime.Internal.Auth;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Utils;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Reflection.Emit;
namespace Dynastio.Bot
{
    /// <summary>
    /// Extension methods for formatting text, working with role/ID values,
    /// stripping HTML, and building Discord embeds.
    /// </summary>
    public static class StringExtensions
    {


        public static string StarIfNullFormat(this string format, params object[] value)
        {
            if (string.IsNullOrEmpty(format)) return "";
            return string.Format(format, value.Select(p => string.IsNullOrEmpty(p?.ToString()) ? "*" : p.ToString() ?? "*")
                .ToArray());
        }






        #region Markdown & Text Formatting



        /// <summary>
        /// Converts a URL and label into a Markdown link: [label](url).
        /// </summary>
        /// <param name="url">The target URL.</param>
        /// <param name="label">The link text.</param>
        /// <returns>A Markdown-formatted hyperlink.</returns>
        public static string ToMarkdownLink(this string url, string label)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL must be provided", nameof(url));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label must be provided", nameof(label));

            return $"[{label}]({url})";
        }

        /// <summary>
        /// Wraps the value in triple backticks (```value```), creating a Markdown code block.
        /// </summary>
        public static string ToCodeBlock(this string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return $"```{value}```";
        }

        /// <summary>
        /// Wraps the value in single backticks (`value`), creating inline code.
        /// </summary>
        public static string ToInlineCode(this string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return $"`{value}`";
        }

        /// <summary>
        /// Wraps the text in **bold** Markdown.
        /// </summary>
        public static string ToBold(this string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return $"**{value}**";
        }

        #endregion

        #region ID & Role Formatting

        /// <summary>
        /// If the ulong value is zero, returns <paramref name="fallback"/>; 
        /// otherwise returns the value as a string.
        /// </summary>
        public static string ToStringOrFallback(this ulong value, string fallback = "Not Available")
        {
            return value == 0 ? fallback : value.ToString();
        }

        /// <summary>
        /// If the role ID is zero, returns <paramref name="fallback"/>;
        /// otherwise returns a Discord role mention.
        /// </summary>
        public static string ToDiscordRoleMention(this ulong roleId, string fallback = "Not Available")
        {
            return roleId == 0
                ? fallback
                : MentionUtils.MentionRole(roleId);
        }

        #endregion

        #region HTML Stripping

        private static readonly Regex _htmlTagRegex =
            new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Removes all HTML tags from the input string.
        /// </summary>
        public static string RemoveHtmlTags(this string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return _htmlTagRegex.Replace(value, string.Empty);
        }

        #endregion

        #region Discord Embed Builders

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> with the provided properties.
        /// Only non-null parameters are applied.
        /// </summary>
        /// <param name="description">The embed description.</param>
        /// <param name="title">Optional title.</param>
        /// <param name="thumbnailUrl">Optional thumbnail URL.</param>
        /// <param name="imageUrl">Optional image URL.</param>
        /// <param name="color">Optional embed color.</param>
        public static EmbedBuilder ToEmbedBuilder(
            this string description,
            string title = null,
            string thumbnailUrl = null,
            string imageUrl = null,
            Color? color = null)
        {
            if (description is null)
                throw new ArgumentNullException(nameof(description));

            var eb = new EmbedBuilder()
                .WithDescription(description);

            if (!string.IsNullOrWhiteSpace(title)) eb.WithTitle(title);
            if (!string.IsNullOrWhiteSpace(thumbnailUrl)) eb.WithThumbnailUrl(thumbnailUrl);
            if (!string.IsNullOrWhiteSpace(imageUrl)) eb.WithImageUrl(imageUrl);
            if (color.HasValue) eb.WithColor(color.Value);

            return eb;
        }

        /// <summary>
        /// Builds a complete <see cref="Embed"/> from the given description and options.
        /// </summary>
        public static Embed ToEmbed(
            this string description,
            string title = null,
            string thumbnailUrl = null,
            string imageUrl = null,
            Color? color = null)
        {
            return description
                .ToEmbedBuilder(title, thumbnailUrl, imageUrl, color)
                .Build();
        }

        /// <summary>
        /// Shorthand for creating an informational embed (orange color).
        /// </summary>
        public static Embed ToInformEmbed(
            this string description,
            string title = null,
            string thumbnailUrl = null,
            string imageUrl = null)
        {
            return description
                .ToEmbedBuilder(title, thumbnailUrl, imageUrl, Color.Orange)
                .Build();
        }

        #endregion
    }
}