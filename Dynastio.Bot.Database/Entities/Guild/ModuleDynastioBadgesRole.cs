using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    public class ModuleDynastioBadgesRole : IGuildRoleModuleBase
    {
        public ModuleDynastioBadgesRole()
        {
            IsEnabled = false;
            HeaderId = 0;
        }
        public bool IsEnabled { get; set; }
        public ulong HeaderId { get; set; }

        public List<BadgeRole> Roles { get; set; } = new();

        public BadgeRole GetRole(BadgeType badge)
        {
            return Roles.FirstOrDefault(a => a.Badge == badge);
        }
        public ulong GetRoleId(BadgeType badge)
        {
            return GetRole(badge).RoleId;
        }
        public void AddOrUpdate(BadgeType badge, ulong id)
        {
            var role = GetRole(badge);
            if (role is null)
            {
                role = new BadgeRole()
                {
                    Badge = badge,
                    RoleId = id
                };
               Roles.Add(role);
            }
            else
            {
                role.RoleId = id;
            }
        }
    }
}
