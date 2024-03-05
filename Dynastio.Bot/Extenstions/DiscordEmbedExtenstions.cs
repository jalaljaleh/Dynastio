using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class DiscordEmbedExtenstions
    {
        public static Embed[] ToArray(this Embed embed)
        {
            return new Embed[] { embed };
        }
    }
}
