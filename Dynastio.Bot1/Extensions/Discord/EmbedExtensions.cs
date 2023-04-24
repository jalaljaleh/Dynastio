using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Extensions.Discord
{
    public static class EmbedExtensions
    {
        public static Embed[] ToEmbeds(this Embed Value)
        {
            return new Embed[] { Value };
        }
    }
}
