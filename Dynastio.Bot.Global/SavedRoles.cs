using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Net;
namespace Dynastio.Bot.Global
{
    public class SavedRoles
    {
        public readonly static Dictionary<BadgeType, ulong> BadgeRoles = new()
        {
            { BadgeType.Developer, 1100680838678581269},
            { BadgeType.Monthly, 1098272223116148796},
            { BadgeType.CupBronze, 1106216604720701460},
            { BadgeType.CupPlatinum, 1106217232821915678},
            { BadgeType.CupSilver, 1106217385217753138},
            { BadgeType.Robot, 1101210991934586941},
            { BadgeType.MapMaker, 1106149689486757898 },
            { BadgeType.Friend, 1100739916410921000},
            { BadgeType.TopDonate, 1100687576324657182},
            { BadgeType.Premium, 1100484334500200511},
            { BadgeType.Supporter, 1100740908846153848},
            { BadgeType.Translator, 1100741214669647992},
            { BadgeType.YoutuberBronze, 1106217618416881664},
            { BadgeType.YoutuberGold, 1106217944054243449},
            { BadgeType.YoutuberPlatinum, 1106218053596872814},
            { BadgeType.YoutuberSilver, 1106218154365034606},
            { BadgeType.Void, 1100741981812051998},
        };
        public readonly static Dictionary<RoleType, ulong> RoleTypes = new()
        {
            { RoleType.Moderator, 1099664231906955385},

        };
        public enum RoleType
        {
            Moderator
        }
    }
}
