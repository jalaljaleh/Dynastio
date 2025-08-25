
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class BadgeBridgeSettings
    {
        public BadgeBridgeSettings()
        {
        }
        public bool IsEnabled { get; set; }
        public string BadgesRoleAssignmentPerfix { get; set; }

        //public List<BadgeRoleItem> Roles { get; set; } = new();

        //public BadgeRoleItem GetItem(int badge)
        //{
        //    return Roles.FirstOrDefault(a => a.Badge == badge);
        //}
        //public bool RemoveItem(int badge = 0, ulong roleId = 0)
        //{
        //    return Roles.Remove(Roles.FirstOrDefault(a => a.Badge == badge || roleId == a.RoleId));
        //}
        //public void AddOrUpdate(BadgeRoleItem badge, ulong id)
        //{
        //    var role = GetItem(badge);
        //    if (role is null)
        //    {
        //        role = new BadgeRoleItem()
        //        {
        //            Badge = badge,
        //            RoleId = id
        //        };
        //        Roles.Add(role);
        //    }
        //    else
        //    {
        //        role.RoleId = id;
        //    }
        //}
    }
    //public class BadgeRoleItem()
    //{
    //    public int Badge { get; set; }
    //    public ulong RoleId { get; set; }
    //}
}
