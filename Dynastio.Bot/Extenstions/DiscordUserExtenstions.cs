using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class DiscordUserExtenstions
    {
        public static string GetAvailableAvatarUrl(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }
    }
}
