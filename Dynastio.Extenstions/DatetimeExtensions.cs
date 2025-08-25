using System;

namespace Dynastio.Bot.Extensions
{
    /// <summary>
    /// Provides extension methods for working with <see cref="DateTime"/> in Discord bots.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a Unix timestamp in seconds.
        /// </summary>
        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            var utc = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
            return (int)(utc - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Formats a <see cref="DateTime"/> into Discord's timestamp format.
        /// Default is relative time (:R), but you can pass any Discord time style.
        /// Docs: https://discord.com/developers/docs/reference#message-formatting-timestamp-styles
        /// </summary>
        public static string ToDiscordTimestamp(this DateTime dateTime, string style = "R")
        {
            return $"<t:{dateTime.ToUnixTimestamp()}:{style}>";
        }

        /// <summary>
        /// Returns a human-friendly relative time string (e.g. "5 minutes ago", "in 2 hours").
        /// </summary>
        public static string ToRelativeString(this DateTime input)
        {
            var utcNow = DateTime.UtcNow;
            var span = utcNow - (input.Kind == DateTimeKind.Utc ? input : input.ToUniversalTime());

            bool isFuture = span.TotalSeconds < 0;
            var delta = Math.Abs(span.TotalMinutes);

            string suffix = isFuture ? " from now" : " ago";

            return delta switch
            {
                < 1 => "a minute" + suffix,
                < 45 => $"{Math.Round(delta)} minutes{suffix}",
                < 90 => "1 hour" + suffix,
                < 1440 => $"{Math.Round(Math.Abs(span.TotalHours))} hours{suffix}",  // < 1 day
                < 2880 => "a day" + suffix,                                          // < 2 days
                < 43200 => $"{Math.Floor(Math.Abs(span.TotalDays))} days{suffix}",    // < 30 days
                < 86400 => "a month" + suffix,                                        // < 60 days
                < 525600 => $"{Math.Floor(Math.Abs(span.TotalDays / 30))} months{suffix}", // < 1 year
                < 1051200 => "a year" + suffix,                                         // < 2 years
                _ => $"{Math.Floor(Math.Abs(span.TotalDays / 365))} years{suffix}"
            };
        }
    }
}
