using System;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Provides handy extensions for truncating and trimming strings safely.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Truncates the given string so that its length does not exceed <paramref name="maxLength"/>.
        /// Optionally appends an ellipsis indicator ("…") when truncation occurs.
        /// </summary>
        /// <param name="value">
        /// The string to truncate. If <c>null</c> is passed, <c>null</c> is returned.
        /// </param>
        /// <param name="maxLength">
        /// The maximum number of characters to keep from the original string.
        /// If this value is less than zero, the original string is returned unchanged.
        /// </param>
        /// <param name="appendEllipsis">
        /// Whether to append the default ellipsis character ("…") when truncation happens.
        /// </param>
        /// <returns>
        /// Either:
        /// - <c>null</c> if <paramref name="value"/> is <c>null</c>,  
        /// - the original string if its length is ≤ <paramref name="maxLength"/> or <paramref name="maxLength"/> is negative,  
        /// - otherwise, the first <paramref name="maxLength"/> characters plus "…" if <paramref name="appendEllipsis"/> is <c>true</c>,  
        ///   or without the ellipsis if <paramref name="appendEllipsis"/> is <c>false</c>.
        /// </returns>
        public static string Truncate(this string value, int maxLength, bool appendEllipsis = true)
        {
            // Null-safe guard
            if (value == null)
                return null;

            // If maxLength is negative, just return the original
            if (maxLength < 0)
                return value;

            // If the string already fits, no change needed
            if (value.Length <= maxLength)
                return value;

            // Perform truncation
            var truncated = value.Substring(0, maxLength);

            // Optionally append the ellipsis
            return appendEllipsis
                ? truncated + "…"
                : truncated;
        }
    }
}
