using Discord;
using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services.XpRankingSystem
{
    public static class XpRankingSystemServiceHelper
    {
        public static async Task<List<IRole>> AssignmentUserRolesAsync(Guild guild, IGuildUser user, int level)
        {
            //// Early exit if disabled
            //if (!guild.XpSystemSettings.IsRankingRoleAssignmentEnabled)
            //    return null;

            // Grab all roles that match the prefix
            var allRoles = RoleHelper
                .GetRolesStartingWith(user.Guild, guild.XpSystemSettings.RankingRolePrefix)
                .ToList();

            allRoles.Reverse();

            // Cap level at the total number of available roles
            int assignCount = Math.Min(level, allRoles.Count);

            // The roles that *should* be on the user
            var desiredRoles = allRoles.Take(assignCount).ToList();

            // The roles the user currently has among these ranking roles
            var currentRoles = desiredRoles.Where(r => user.RoleIds.Contains(r.Id)).ToList();

            // Roles missing from the user
            var toAdd = desiredRoles.Except(currentRoles).ToList();

            // (Optional) Roles the user has but no longer needs
            var toRemove = allRoles
                .Where(r => user.RoleIds.Contains(r.Id) && !desiredRoles.Contains(r))
                .ToList();

            // Add new ranking roles
            foreach (var role in toAdd)
            {
                await user.AddRoleAsync(role.Id);
                await Task.Delay(1000);
            }

            // Remove outdated ranking roles (optional)
            foreach (var role in toRemove)
                await user.RemoveRoleAsync(role.Id);

            // Ensure header role is present
            var headerRole = RoleHelper
                .GetNextHigherHeaderRole(user.Guild, guild.XpSystemSettings.RankingRolePrefix);

            if (headerRole != null && !user.RoleIds.Contains(headerRole.Id))
                await user.AddRoleAsync(headerRole.Id);

            return toAdd;
        }
    }
}
