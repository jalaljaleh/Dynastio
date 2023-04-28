using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class DatetimeExtensions
    {
        public static Int32 ToUnixTimestamp(this DateTime datetime)
        {
            return (int)datetime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static string ToDiscordUnixTimestampFormat(this DateTime datetime)
        {
            return $"<t:{datetime.ToUnixTimestamp()}:R>";
        }
        public static string ToRelative(this DateTime input)
        {
            TimeSpan oSpan = DateTime.UtcNow.Subtract(input);
            double TotalMinutes = oSpan.TotalMinutes;
            string Suffix = " ago";

            if (TotalMinutes < 0.0)
            {
                TotalMinutes = Math.Abs(TotalMinutes);
                Suffix = " from now";
            }

            var aValue = new SortedList<double, Func<string>>();
            aValue.Add(0.75, () => "a minute");
            aValue.Add(1.5, () => "a minute");
            aValue.Add(45, () => string.Format("{0} minutes", Math.Round(TotalMinutes)));
            aValue.Add(90, () => "1 hour");
            aValue.Add(1440, () => string.Format("{0} hours", Math.Round(Math.Abs(oSpan.TotalHours)))); // 60 * 24
            aValue.Add(2880, () => "a day"); // 60 * 48
            aValue.Add(43200, () => string.Format("{0} days", Math.Floor(Math.Abs(oSpan.TotalDays)))); // 60 * 24 * 30
            aValue.Add(86400, () => "a month"); // 60 * 24 * 60
            aValue.Add(525600, () => string.Format("{0} months", Math.Floor(Math.Abs(oSpan.TotalDays / 30)))); // 60 * 24 * 365 
            aValue.Add(1051200, () => "a year"); // 60 * 24 * 365 * 2
            aValue.Add(double.MaxValue, () => string.Format("{0} years", Math.Floor(Math.Abs(oSpan.TotalDays / 365))));

            return aValue.First(n => TotalMinutes < n.Key).Value.Invoke() + Suffix;
        }
    }
}
