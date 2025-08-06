using Discord;
using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services.XpRankingSystem
{
    public static class RoleSyncService
    {
        public static async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, int level)
        {
            if (!guild.XpSystemSettings.IsRankingRoleAssignmentEnabled) return false;

            var allRoles = RoleHelper.GetRolesStartWith(user.Guild, guild.XpSystemSettings.RankingRoleAssignmentPerfix);
            var userRoles = allRoles.Where(role => user.RoleIds.Contains(role.Id)).ToList();

            if (userRoles.Count == level || userRoles.Count == allRoles.Count || allRoles.Count <= level)
                return true;

            var headerRole = RoleHelper.GetNextHigherRole(user.Guild, guild.XpSystemSettings.RankingRoleAssignmentPerfix);
            if (!userRoles.Contains(headerRole) && userRoles.Count > 0)
                await user.AddRoleAsync(headerRole.Id);

            for (int i = 0; i < level; i++)
            {
                var targetRole = allRoles[i];
                if (!user.RoleIds.Contains(targetRole.Id))
                {
                    await user.AddRoleAsync(targetRole);
                    await Task.Delay(1000);
                }
            }

            return true;
        }
    }
}
