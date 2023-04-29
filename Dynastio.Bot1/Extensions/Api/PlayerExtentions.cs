using Discord;
using Dynastio.Net;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class PlayerExtentions
    {
        public static List<SelectMenuOptionBuilder> ToSelectMenuOptionBuilder(this List<Player> Players, int Index)
        {
            List<SelectMenuOptionBuilder> options = new();
            foreach (var player in Players)
            {
                var o = new SelectMenuOptionBuilder()
                    .WithValue(player.UniqeId)
                    .WithLabel((Players.IndexOf(player) + Index).ToRegularCounter() + ". " + StringExtensions.TrySubstring(player.Nickname.RemoveLines(), 16))
                    .WithDescription(player.IsAuth ? (player.IsDiscordAuth() ? "Discord Player" : "Google or Facebook Player") : "Guest")
                    .WithEmote(player.IsAuth ? new Emoji("🔸") : new Emoji("🔹"));
                options.Add(o);
            }
            return options;
        }
    }
}
