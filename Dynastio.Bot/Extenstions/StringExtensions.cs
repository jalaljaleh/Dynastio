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
            return Regex.Replace(value, "<.*?>", string.Empty);
        }
        public static string ToBold(this string value)
        {
            return $"**{value}**";
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
        public static Embed ToEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null, Color color = default)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl, color).Build();
        }
        public static Embed ToInformEmbed(this string value, string title = null, string thumbnailUrl = null, string imageUrl = null)
        {
            return value.ToEmbedBuilder(title, thumbnailUrl, imageUrl, Color.Orange).Build();
        }
        public static Embed ToEmbed(this string value, string title, Color color)
        {
            return value.ToEmbedBuilder(title, default, default, color).Build();
        }
        public static string ToMarkdown(this string value) => $"```{value}```";
        public static string ToCodeBlocks(this string value) => $"`{value}`";
    }
}
