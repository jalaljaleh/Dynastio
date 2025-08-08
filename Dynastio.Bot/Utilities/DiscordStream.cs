using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Webhook;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace Dynastio.Bot.Addons
{
    public enum ImageFormat
    {
        Png,
        Jpeg
    }

    public static class DiscordStreamExtensions
    {
        // ─────────────── SEND IMAGE AS FILE ───────────────

        /// <summary>
        /// Sends an ImageSharp Image as a file to any IMessageChannel.
        /// </summary>
        public static async Task<IUserMessage> SendImageAsync(
            this IMessageChannel channel,
            Image image,
            string fileName,
            string caption = null,
            bool isTTS = false,
            Embed embed = null,
            bool isSpoiler = false,
            AllowedMentions allowedMentions = null,
            MessageReference messageReference = null,
            MessageComponent components = null,
            ISticker[] stickers = null,
            Embed[] embeds = null,
            MessageFlags flags = MessageFlags.None,
            RequestOptions options = null,
            ImageFormat format = ImageFormat.Png)
        {
            await using var ms = new MemoryStream();

            // Encode into selected format
            switch (format)
            {
                case ImageFormat.Png:
                    await image.SaveAsPngAsync(ms, new PngEncoder());
                    break;
                case ImageFormat.Jpeg:
                    await image.SaveAsJpegAsync(ms, new JpegEncoder());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            ms.Position = 0;
            image.Dispose();

            return await channel.SendFileAsync(
                ms,
                fileName,
                caption,
                isTTS,
                embed,
                options,
                isSpoiler,
                allowedMentions,
                messageReference,
                components,
                stickers,
                embeds,
                flags
            ).ConfigureAwait(false);
        }

        // ─────────────── FOLLOW UP WITH IMAGE ───────────────

        /// <summary>
        /// Follows up an interaction with an ImageSharp Image as a file.
        /// </summary>
        public static async Task<IUserMessage> FollowupWithImageAsync(
            this IInteractionContext ctx,
            Image image,
            string fileName,
            string text = null,
            bool isTTS = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            MessageComponent components = null,
            Embed embed = null,
            RequestOptions options = null,
            ImageFormat format = ImageFormat.Png)
        {
            await using var ms = new MemoryStream();

            switch (format)
            {
                case ImageFormat.Png:
                    await image.SaveAsPngAsync(ms, new PngEncoder());
                    break;
                case ImageFormat.Jpeg:
                    await image.SaveAsJpegAsync(ms, new JpegEncoder());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            ms.Position = 0;
            image.Dispose();

            return await ctx.Interaction.FollowupWithFileAsync(
                ms, fileName, text,
                embeds: null,
                isTTS,
                ephemeral,
                allowedMentions,
                components,
                embed,
                options
            ).ConfigureAwait(false);
        }

        // ─────────────── SEND STRING AS FILE ───────────────

        /// <summary>
        /// Sends a plain text string as a .txt file.
        /// </summary>
        public static async Task<IUserMessage> SendStringAsFileAsync(
            this IMessageChannel channel,
            string content,
            string fileName = "text.txt",
            RequestOptions options = null)
        {
            await using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, leaveOpen: true);

            writer.Write(content);
            await writer.FlushAsync().ConfigureAwait(false);

            ms.Position = 0;
            return await channel.SendFileAsync(
                ms,
                fileName,
                options: options
            ).ConfigureAwait(false);
        }
    }
}