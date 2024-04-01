using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Extenstions
{
    public static class AdvertiseExtentions
    {
        public static ButtonBuilder ToButtonBuilder(this Advertise advertise)
        {
            return new ButtonBuilder(
                 advertise.Label,
                 null,
                 ButtonStyle.Link,
                 advertise.Url,
                 emote: advertise.HasEmoji() ? new Emoji(advertise.Emoji) : null,
                 false);
        }
        public static ButtonBuilder[] TobuttonBuilders(this Advertise[] advertises) => 
            advertises.Select(a => a.ToButtonBuilder()).ToArray();

    }
}
