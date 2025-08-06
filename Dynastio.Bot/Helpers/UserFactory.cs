using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class UserFactory
    {
        public static User CreateDefault(ulong Id)
        {
            return new User()
            {
                Id = Id,
                Accounts = new(),
                GuildProfiles = new()
            };
        }
    }
}
