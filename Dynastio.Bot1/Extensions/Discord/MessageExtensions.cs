using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class MessageExtensions
    {
        public static bool HasAnyImage(this IMessage message)
        {
            return message.Attachments.Where(a => a.ContentType.Contains("image")).Any();
        }
    }
}
