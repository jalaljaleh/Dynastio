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
                if (r.isSuccesful && r.result != null)
                    profiles.Add(r.result);

                await Task.Delay(500);
            }
            return profiles;
        }
        public async Task<List<BadgeType>> GetUserBadgesAsync(User user)
        {
            var badges = await GetUserProfilesAsync(user);
            return badges.Where(a => a.Badges != null && a.Badges.Count > 0).SelectMany(a => a.Badges).ToList();
        }
        public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, User buser)
        {
            if (guild.BadgesRole.IsEnabled is false) return false;
            if (guild.BadgesRole.HeaderId == 0) return false;

            var playerBadge = await GetUserBadgesAsync(buser);

            foreach (var roleBadge in guild.BadgesRole.Roles.Where(a => user.RoleIds.Contains(a.RoleId) || playerBadge.Contains(a.Badge)))
            {

                bool HasRole = user.RoleIds.Contains(roleBadge.RoleId);
                bool HasBadge = playerBadge.Contains(roleBadge.Badge);

                if (HasRole && !HasBadge)
                    await user.RemoveRoleAsync(roleBadge.RoleId);

                if (!HasRole && HasBadge)
                    await user.AddRoleAsync(roleBadge.RoleId);

                await Task.Delay(180);
            }

            var roleIds = guild.BadgesRole.Roles.Select(a => a.RoleId);
            bool HasHeaderRole = user.RoleIds.Contains(guild.BadgesRole.HeaderId);
            bool HasAnyBadgeRole = user.RoleIds.Any(a => roleIds.Contains(a));

            if (HasAnyBadgeRole && !HasHeaderRole)
                await user.AddRoleAsync(guild.BadgesRole.HeaderId);
            else if (!HasAnyBadgeRole && HasHeaderRole)
                await user.RemoveRoleAsync(guild.BadgesRole.HeaderId);

            return true;
        }


    }
}
