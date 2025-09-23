using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Services;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class DiscordResolver
    {
        /// <summary>
        /// Resolves a Discord user from a Player object,
        /// either via direct Discord auth or account lookup.
        /// </summary>
        public static async Task<IUser> ResolveDiscordUserAsync(Player player, DiscordSocketClient client, UsersService usersService)
        {
            if (player.IsDiscordAuth)
            {
                var userId = ulong.Parse(player.Id.Replace("discord:", ""));
                return await client.GetUserAsync(userId);
            }

            var botUser = await usersService.GetUserByAccountIdAsync(player.Id);
            return botUser == null
                ? null
                : await client.GetUserAsync(botUser.Id);
        }
    }
}
