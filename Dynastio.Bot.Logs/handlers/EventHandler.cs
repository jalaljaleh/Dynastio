using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot.Events
{
    public class EventsHandler
    {
        private readonly DiscordSocketClient _discord;
        public EventsHandler(DiscordSocketClient discord)
        {
            _discord = discord;
            _discord.Ready += _discord_Ready;
        }

        private async Task _discord_Ready()
        {
            await _discord.SetStatusAsync(UserStatus.Online);
            await _discord.SetGameAsync(_discord.Guilds.FirstOrDefault()?.MemberCount + " Members", "https://www.youtube.com/watch?v=v74AQTvjtSg", ActivityType.Watching);
        }

    }
}
