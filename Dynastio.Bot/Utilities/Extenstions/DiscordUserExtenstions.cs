using Discord;
using Discord.WebSocket;
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
        public static string TryGetAvatarUrl(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }
        public static Color TryGetRoleColor(this IGuildUser user)
        {
            return (user as SocketGuildUser)?.Roles?.Where(a => a.IsHoisted)?.OrderByDescending(a => a.Position)?.FirstOrDefault()?.Color ?? Color.Default;
        }
        
    }
}
