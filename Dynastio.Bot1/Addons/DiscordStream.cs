using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Discord.Webhook;

namespace Dynastio.Bot
{
    public class DiscordStream
    {
        public static async Task<IUserMessage> FollowupWithFileAsync(IInteractionContext Context, SixLabors.ImageSharp.Image img, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            IUserMessage msg;
            using (var stream = new System.IO.MemoryStream())
            {
                try
                {
                    img.SaveAsJpeg(stream);
                    img.Dispose();
                    msg = await Context.Interaction.FollowupWithFileAsync(stream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
                }
                finally
                {
                    stream.Dispose();
                }
            }
            return msg;
        }

        public static async Task<IUserMessage> SendStringAsFile(IMessageChannel channel, string value, string fileName = "text.txt")
        {
            IUserMessage res;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(value);
                    writer.Flush();
                    stream.Position = 0;
                    res = await channel.SendFileAsync(stream: stream, filename: fileName);
                }

            }
            return res;
        }
    }
}
