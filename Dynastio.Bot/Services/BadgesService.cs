using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
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
        public void Initialize()
        {

        }
        public async Task<List<Profile>> GetUserProfilesAsync(User user)
        {
            List<Profile> profiles = new List<Profile>();
            foreach (var profile in user.Accounts)
            {
                var r = await this._dynast.GetUserProfileAsync(profile.Id).TryAsync<Profile>();
                if (r.isSuccesful)
                    profiles.Add(r.result);

                await Task.Delay(500);
            }
            return profiles;
        }
        public async Task<List<BadgeType>> GetUserBadgesAsync(User user)
        {
            var badges = await GetUserProfilesAsync(user);
            return badges.SelectMany(a => a.Badges).ToList();
        }
        public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, User buser)
        {
            if (guild.BadgeRoles.IsEnabled is false) return false;
            if (guild.BadgeRoles.HeaderId != 0) return false;

            var badges = await GetUserBadgesAsync(buser);

            foreach (var pair in guild.BadgeRoles.RolesId)
            {
                if (badges.Contains(pair.Key))
                {
                    try
                    {
                        // user have the role but not the badge
                        if (user.RoleIds.Contains(pair.Value) && !badges.Contains(pair.Key))
                        {
                            await user.RemoveRoleAsync(pair.Value);
                        }  // user have the badge but not the role
                        else if (!user.RoleIds.Contains(pair.Value) && badges.Contains(pair.Key))
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
