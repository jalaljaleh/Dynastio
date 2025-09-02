using System;

namespace Dynastio.Bot
{
    /// <summary>
    /// Extension methods for formatting integers and large numbers.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Converts an integer to a two-digit string (e.g., 7 → "07").
        /// </summary>
        public static string ToRegularCounter(this int number)
        {
            if (number == 0) number = 1;
            return number < 10 ? $"0{number}" : number.ToString();
        }

        /// <summary>
        /// Converts a numeric value into a metric abbreviation (e.g., 1500 → "1.5K").
        /// </summary>
        public static string ToMetric(this ulong value)
        {
            if (value < 1) value = 1;
            if (value < 1_000) return value.ToString();

            return value switch
            {
                >= 100_000_000_000_000_000 => $"{Math.Round(value / 1e18, 1)}E",
                >= 100_000_000_000_000 => $"{Math.Round(value / 1e15, 1)}P",
                >= 100_000_000_000 => $"{Math.Round(value / 1e12, 1)}T",
                >= 1_000_000_000 => $"{Math.Round(value / 1e9, 1)}G",
                >= 1_000_000 => $"{Math.Round(value / 1e6, 1)}M",
                _ => $"{Math.Round(value / 1e3, 1)}K"
            };
        }

        /// <summary>
        /// Converts a long value into a metric abbreviation.
        /// </summary>
        public static string ToMetric(this long value)
        {
            return ((ulong)Math.Max(value, 1)).ToMetric();
        }

        /// <summary>
        /// Converts an int value into a metric abbreviation.
        /// </summary>
        public static string ToMetric(this int value)
        {
            return ((ulong)Math.Max(value, 1)).ToMetric();
        }
        public static string ToMetric(this double value)
        {
            return ((ulong)Math.Max(value, 1)).ToMetric();
        }
    }
}