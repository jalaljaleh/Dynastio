using Discord;
using Dynastio.Net;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class GuildExtentions
    {
        public static List<SelectMenuOptionBuilder> ToSelectMenuOptionBuilder(this List<IGuildChannel> Channels, int Index)
        {
            List<SelectMenuOptionBuilder> options = new();
            foreach (var channel in Channels)
            {
                var o = new SelectMenuOptionBuilder()
                    .WithValue(channel.Id.ToString())
                    .WithLabel((Channels.IndexOf(channel) + Index).ToRegularCounter() + ". " + channel.Name)
                    .WithDescription(channel.Id.ToString())
                    .WithEmote(new Emoji("💬"));
                options.Add(o);
            }
            return options;
        }
    }
}
