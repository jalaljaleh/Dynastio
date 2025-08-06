using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class RoleHelper
    {
        public static List<IRole> GetRolesStartWith(IGuild guild, string rolePrefix)
        {
            var matchingRoles = guild.Roles
                .Where(r => r.Name.StartsWith(rolePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Position)
                .ToList();

            return matchingRoles;
        }
        public static IRole? GetNextHigherRole(IGuild guild, string rolePrefix)
        {
            var matchingRoles = GetRolesStartWith(guild, rolePrefix);

            if (matchingRoles.Count == 0)
                return null;

            var highestMatching = matchingRoles.First();

            var nextHigher = guild.Roles
                .Where(r => r.Position > highestMatching.Position)
                .OrderBy(r => r.Position)
                .FirstOrDefault();

            return nextHigher;
        }
    }
}
