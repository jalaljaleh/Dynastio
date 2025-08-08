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
        public static async Task<bool> AssignmentUserRolesAsync(Guild guild, IGuildUser user, int level)
        {
            if (!guild.XpSystemSettings.IsRankingRoleAssignmentEnabled) return false;

            var allRoles = RoleHelper.GetRolesStartingWith(user.Guild, guild.XpSystemSettings.RankingRoleAssignmentPerfix);
            var userRoles = allRoles.Where(role => user.RoleIds.Contains(role.Id)).ToList();

            var headerRole = RoleHelper.GetPrefixHeaderRole(user.Guild, guild.XpSystemSettings.RankingRoleAssignmentPerfix);
            if (headerRole != null && !user.RoleIds.Contains(headerRole.Id) && userRoles.Count > 0)
                await user.AddRoleAsync(headerRole.Id);

            // Limit level to available roles
            int maxAssignable = Math.Min(level, allRoles.Count);

            for (int i = 0; i < maxAssignable; i++)
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
