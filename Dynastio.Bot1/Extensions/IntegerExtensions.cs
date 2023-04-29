using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class IntegerExtensions
    {

        public static string ToDiscordUnixTimeFromat(this int Value)
        {
            return $"<t:{(DateTime.UtcNow + TimeSpan.FromSeconds(Value)).ToUnixTimestamp()}:R>";
        }
        public static string ToUserMention(this ulong Id)
        {
            return Discord.MentionUtils.MentionUser(Id);
        }
    }
}
