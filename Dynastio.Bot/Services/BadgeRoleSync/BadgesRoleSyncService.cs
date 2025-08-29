using Discord;
using Dynastio.Bot.Database;
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
    public class BadgesRoleSyncService 
    {
        private readonly DynastioApi _dynast;
        public BadgesRoleSyncService(IServiceProvider services)
        {
            _dynast = services.GetService<DynastioApi>();
        }
        public async Task<List<Profile>> GetUserProfilesAsync(User user)
        {
            List<Profile> profiles = new List<Profile>();
            foreach (var profile in user.Accounts)
            {
                var result = await _dynast.GetUserProfileAsync(profile.Id).TryAsync();
                if (result.isSuccessful && result.result != null)
                    profiles.Add(result.result);

                await Task.Delay(500);
            }
            return profiles;
        }
        public async Task<List<string>> GetUserBadgesAsync(User user)
        {
            var profiles = await GetUserProfilesAsync(user);

            // Get distinct badge enum values from profiles
            var enumBadges = profiles
                .Where(a => a.Badges != null && a.Badges.Count > 0)
                .SelectMany(a => a.Badges)
                .Distinct()
                .ToList();

            // Convert each badge enum value to its name as string
            var stringBadges = enumBadges
                .Select(b => b.ToString())
                .ToList();

            return stringBadges;
        }

        public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, User buser)
        {
            if (!guild.BadgeBridgeSettings.IsEnabled) return false;

            var playerBadges = await GetUserBadgesAsync(buser); // string list

            var badgeRoles = RoleHelper.GetRolesStartingWith(user.Guild, guild.BadgeBridgeSettings.BadgeRoleAssignmentPrefix);

            foreach (var role in badgeRoles)
            {
                string badgeName = role.Name.ToBadgeEnumAble(guild.BadgeBridgeSettings.BadgeRoleAssignmentPrefix);

                bool hasBadge = playerBadges.Contains(badgeName, StringComparer.OrdinalIgnoreCase);
                bool hasRole = user.RoleIds.Contains(role.Id);

                if (hasRole && !hasBadge)
                    await user.RemoveRoleAsync(role.Id);

                if (!hasRole && hasBadge)
                    await user.AddRoleAsync(role.Id);

                await Task.Delay(180);
            }
            var badgeRoleIds = badgeRoles.Select(r => r.Id);
            bool hasAnyBadgeRole = user.RoleIds.Any(id => badgeRoleIds.Contains(id));

            var headerRoleId = RoleHelper.GetNextHigherHeaderRole(user.Guild, guild.BadgeBridgeSettings.BadgeRoleAssignmentPrefix);
            bool hasHeaderRole = user.RoleIds.Contains(headerRoleId.Id);

            if (hasAnyBadgeRole && !hasHeaderRole)
                await user.AddRoleAsync(headerRoleId.Id);

            else if (!hasAnyBadgeRole && hasHeaderRole)
                await user.RemoveRoleAsync(headerRoleId.Id);

            return true;
        }


        //public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, User buser)
        //{
        //    if (guild.BadgeBridgeSettings.IsEnabled is false) return false;

        //    var playerBadge = await GetUserBadgesAsync(buser);
        //    var serverBadgeRoles = RoleHelper.GetRolesStartWith(user.Guild, guild.BadgeBridgeSettings.BadgesRoleAssignmentPerfix);
        //    var serverBadgeHeaderRole = RoleHelper.GetNextHigherRole(user.Guild, guild.BadgeBridgeSettings.BadgesRoleAssignmentPerfix);

        //    foreach (var badge in playerBadge)
        //    {
        //        bool HasRole = user.RoleIds.Contains(badge.RoleId);
        //        bool HasBadge = playerBadge.Contains(badge.Badge);

        //        if (HasRole && !HasBadge)
        //            await user.RemoveRoleAsync(badge.RoleId);

        //        if (!HasRole && HasBadge)
        //            await user.AddRoleAsync(badge.RoleId);

        //        await Task.Delay(180);
        //    }

        //    var roleIds = guild.BadgeBridgeSettings.Roles.Select(a => a.RoleId);
        //    bool HasHeaderRole = user.RoleIds.Contains(guild.BadgeBridgeSettings.BadgesRoleAssignmentHeaderId);
        //    bool HasAnyBadgeRole = user.RoleIds.Any(a => roleIds.Contains(a));

        //    if (HasAnyBadgeRole && !HasHeaderRole)
        //        await user.AddRoleAsync(guild.BadgeBridgeSettings.BadgesRoleAssignmentHeaderId);
        //    else if (!HasAnyBadgeRole && HasHeaderRole)
        //        await user.RemoveRoleAsync(guild.BadgeBridgeSettings.BadgesRoleAssignmentHeaderId);

        //    return true;
        //}


    }
}
