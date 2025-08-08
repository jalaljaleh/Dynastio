using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// Extension methods for working with YouTube-related identifiers.
    /// </summary>
    public static class YouTubeExtensions
    {
        /// <summary>
        /// Converts a YouTube channel ID into a full channel URL.
        /// </summary>
        /// <param name="channelId">The YouTube channel ID.</param>
        /// <returns>The full URL to the YouTube channel.</returns>
        public static string ToYouTubeChannelUrl(this string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

            return $"https://www.youtube.com/channel/{channelId}";
        }

    }
}
