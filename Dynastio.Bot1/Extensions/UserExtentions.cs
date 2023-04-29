using Discord;
using Discord.WebSocket;
using Dynastio.Net;
using Dynastio.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Data;

namespace Dynastio.Bot
{
    public static class UserExtentions
    {

        public static async Task<List<Personalchest>> GetPersonalchests(this List<UserAccount> Accounts, IDynastioProvider client)
        {
            var list = new List<Personalchest>();
            foreach (var a in Accounts)
            {
                var result = await client.GetUserPersonalchestAsync(a.Id).TryGet();
                if (result != null)
                    list.Add(result);
            }
            return list;
        }
      
        public static List<SelectMenuOptionBuilder> ToSelectMenuOptionBuilder(this List<UserAccount> Accounts)
        {
            List<SelectMenuOptionBuilder> options = new();
            foreach (var account in Accounts)
            {
                var o = new SelectMenuOptionBuilder()
                    .WithValue(account.Nickname)
                    .WithLabel(account.Nickname)
                    .WithDescription("Added " + account.AddedAt.ToRelative())
                    .WithEmote(new Emoji("🔸"));
                options.Add(o);
            }
            return options;
        }

    }
}
