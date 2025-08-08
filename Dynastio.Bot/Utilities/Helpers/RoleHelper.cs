using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynastio.Bot
{
    /// <summary>
    /// Provides utility methods for working with Discord roles.
    /// </summary>
    public static class RoleHelper
    {
        /// <summary>
        /// Converts a role name to a badge-compatible enum string by removing a prefix and whitespace.
        /// </summary>
        /// <param name="roleName">The full role name.</param>
        /// <param name="prefix">The prefix to remove.</param>
        /// <returns>A cleaned string suitable for enum conversion.</returns>
        public static string ToBadgeEnumAble(this string roleName, string prefix)
        {
            return roleName.Replace(prefix, "", StringComparison.OrdinalIgnoreCase)
                           .Replace(" ", "")
                           .Trim();
        }

        /// <summary>
        /// Retrieves all roles from the guild that start with the specified prefix.
        /// </summary>
        /// <param name="guild">The guild to search.</param>
        /// <param name="rolePrefix">The prefix to match.</param>
        /// <returns>A list of matching roles, ordered by descending position.</returns>
        public static List<IRole> GetRolesStartingWith(IGuild guild, string rolePrefix)
        {
            return guild.Roles
                        .Where(role => role.Name.StartsWith(rolePrefix, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(role => role.Position)
                        .ToList();
        }

        /// <summary>
        /// Finds the next higher role above the highest role that matches the given prefix.
        /// </summary>
        /// <param name="guild">The guild to search.</param>
        /// <param name="rolePrefix">The prefix to match.</param>
        /// <returns>The next higher role, or null if none found.</returns>
        public static IRole? GetPrefixHeaderRole(IGuild guild, string rolePrefix)
        {
            var matchingRoles = GetRolesStartingWith(guild, rolePrefix);

            if (!matchingRoles.Any())
                return null;

            var highestMatching = matchingRoles.First();

            return guild.Roles
                        .Where(role => role.Position > highestMatching.Position)
                        .OrderBy(role => role.Position)
                        .FirstOrDefault(a => a.Name.ToLower().Contains("rank"));
        }
    }
}