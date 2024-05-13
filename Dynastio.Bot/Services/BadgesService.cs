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

            var userBadges = await GetUserBadgesAsync(buser);

            foreach (var role in guild.BadgeRoles.Roles)
            {
                if (userBadges.Contains(role.Badge)) // has the badge
                {
                    if (user.RoleIds.Contains(role.RoleId)) // user has the role
                        continue;
                    await user.AddRoleAsync(role.RoleId);
                }
                else // hasn't the badge
                {
                    if (user.RoleIds.Contains(role.RoleId)) // user has the role
                        await user.RemoveRoleAsync(role.RoleId);
                }
                await Task.Delay(250);
            }
            return true;
        }


    }
}
