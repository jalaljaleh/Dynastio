using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynastio.Bot
{
    public static class RoleHelper
    {

        /// <summary> 
        /// Converts a role name to a badge-compatible enum string by removing a prefix and whitespace. 
        /// </summary> /// <param name="roleName">The full role name.</param> 
        /// <param name="prefix">The prefix to remove.</param> 
        /// <returns>A cleaned string suitable for enum conversion.</returns> 
        public static string ToBadgeEnumAble(this string roleName, string prefix)
        {
            return roleName
                .Replace(prefix, "", StringComparison.OrdinalIgnoreCase)
                .Replace(" ", "").Trim();
        }

        /// <summary>
        /// Finds the first guild role whose Position is strictly greater than
        /// the highest Position among all roles that start with the given prefix.
        /// </summary>
        public static IRole? GetRoleAbovePrefix(IGuild guild, string prefix)
        {
            // all roles matching your prefix, ordered by position
            var prefixRoles = GetRolesWithPrefix(guild, prefix);
            if (!prefixRoles.Any())
                return null;

            // highest position in that series
            var maxPos = prefixRoles.Max(r => r.Position);

            // the next role above it
            return guild.Roles
                        .Where(r => r.Position > maxPos)
                        .OrderBy(r => r.Position)
                        .FirstOrDefault();
        }


        /// <summary>
        /// Returns all guild roles whose Name starts with the given prefix, ordered by Position ascending.
        /// </summary>
        public static List<IRole> GetRolesWithPrefix(IGuild guild, string prefix)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            return guild.Roles
                        .Where(r => r.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(r => r.Position)
                        .ToList();
        }

        /// <summary>
        /// Returns only those roles (from the prefixed set) that the user actually has.
        /// </summary>
        public static List<IRole> GetUserRolesWithPrefix(IGuildUser user, string prefix)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            // Reuse the guild-level method, then filter by membership
            return GetRolesWithPrefix(user.Guild, prefix)
                   .Where(r => user.RoleIds.Contains(r.Id))
                   .ToList();
        }

        /// <summary>
        /// From the roles the user has (with that prefix), returns the highest one by Position.
        /// </summary>
        public static IRole GetHighestUserRoleWithPrefix(IGuildUser user, string prefix)
        {
            return GetUserRolesWithPrefix(user, prefix)
                   .OrderBy(r => r.Position)
                   .LastOrDefault();
        }

        /// <summary>
        /// From the full prefixed list, returns the very next role above the user's highest one.
        /// </summary>
        public static IRole GetNextRoleWithPrefix(IGuildUser user, string prefix)
        {
            // All roles with prefix, sorted by Position
            var all = GetRolesWithPrefix(user.Guild, prefix);

            // User's highest one (or null if none)
            var highest = GetHighestUserRoleWithPrefix(user, prefix);

            // Take the first role whose Position is strictly greater than highest.Position
            return all
                .Where(r => highest == null || r.Position > highest.Position)
                .OrderBy(r => r.Position)
                .FirstOrDefault();
        }
    }
}
