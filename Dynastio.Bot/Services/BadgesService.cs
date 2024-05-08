using Discord;
using Dynastio.Bot.Database;
using Dynastio.Net;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class BadgesService : ServicesBase
    {
        public BadgesService(IServiceProvider services) : base(services)
        {

        }

        public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, Profile profile)
        {
            if (guild.BadgeRoles.IsEnabled is false) return false;
            if (guild.BadgeRoles.HeaderId != 0) return false;

            foreach (var pair in guild.BadgeRoles.RolesId)
            {
                if (profile.Badges.Contains(pair.Key))
                {
                    try
                    {
                        // user have the role but not the badge
                        if (user.RoleIds.Contains(pair.Value) && !profile.Badges.Contains(pair.Key))
                        {
                            await user.RemoveRoleAsync(pair.Value);
                        }  // user have the badge but not the role
                        else if (!user.RoleIds.Contains(pair.Value) && profile.Badges.Contains(pair.Key))
                        {
                            await user.AddRoleAsync(pair.Value);
                        }
                    }
                    catch
                    {
                        // guild.BadgeRoles.IsEnabled = false;
                    }
                    await Task.Delay(1000);
                }
            }
            return true;
        }
    }
}
