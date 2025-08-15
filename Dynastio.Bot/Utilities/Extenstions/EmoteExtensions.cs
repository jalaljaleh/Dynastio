using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Dynastio.Bot
{

    /// <summary>
    /// Extension methods for working with Discord emotes.
    /// </summary>
    public static class EmoteExtensions
    {
        /// <summary>
        /// Converts an <see cref="Emote"/> to its Discord message tag format.
        /// Example: &lt;:name:id&gt; or &lt;a:name:id&gt; for animated.
        /// </summary>
        /// <param name="emote">The emote to convert.</param>
        /// <returns>The Discord tag string, or empty if null.</returns>
        public static string ToDiscordTag(this Emote emote)
        {
            if (emote == null)
                return string.Empty;

            var prefix = emote.Animated ? "<a:" : "<:";
            return $"{prefix}{emote.Name}:{emote.Id}>";
        }
    }
}


