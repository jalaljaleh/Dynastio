using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class StringExtensions
    {
        public static string RemoveHtmlTags(this string value)
        {
            return Regex.Replace(value, "<.*?>", String.Empty);
        }
        public static string ToBold(this string value)
        {
            return $"**{value}**";
        }
        public static string Join(this IEnumerable<string> value, string spearator)
        {
            return string.Join(spearator, value);
        }

        public static string Remove(this string value, params string[] txt)
        {
            foreach (var x in txt)
                value = value.Replace(x, "");
            return value;
        }
        public static string RemoveLines(this string Value)
        {
            return Regex.Replace(Value, "\\t|\\n|\\r|\\r\\n|\\n\\r|", "");
        }

        public static string TrySubstring(this string value, int maxLength, bool dots = true)
        {
            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength) + (dots ? ".." : "");
            }
            return value;
        }
        public static EmbedBuilder ToEmbedBuilder(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null, Color color = default)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Description = value;
            builder.Title = title;
            builder.ThumbnailUrl = thumbnailUrl;
            builder.ImageUrl = imageUrl;
            builder.Color = color;
            return builder;
        }
        public static Embed ToEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl).Build();
        }
        public static Embed ToSuccessfulEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl, Color.Green).Build();
        }
        public static Embed ToWarnEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl, Color.Orange).Build();
        }
        public static Embed ToDangerEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl, Color.Red).Build();
        }
        public static string ToMarkdown(this string value) => $"```{value}```";
        public static string ResourcesPath(this string path)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), $@"Resources/{path}");
        }
    }
}
