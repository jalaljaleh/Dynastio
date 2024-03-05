using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Helpers
{
    internal class EmbedsHelper
    {
        public static Color ColorWaitingResopnse = 5822463;

        public static EmbedBuilder GetFaildOperatorTemplate()
        {
            return new EmbedBuilder()
            {
                Color = Color.DarkRed,
                
            };
        }

    }
}
