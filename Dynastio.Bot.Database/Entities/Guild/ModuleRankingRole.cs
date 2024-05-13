using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class ModuleRankingRole : IGuildRoleModuleBase
    {
        public ModuleRankingRole()
        {
            IsEnabled = false;
            ChannelIds = new();
            IsGameRewardEnabled = false;
            LogChannelId = 0;
            RolesPrefix = "rank: ";
            Delay = 60;
            XpBoosters = 10;
            XpPerMessage = 30;
            XpRandom = 10;
        }
        public bool IsEnabled { get; set; }
        public ulong HeaderId { get; set; }

        public bool IsGameRewardEnabled { get; set; }
        public int XpPerMessage { get; set; }
        public int XpBoosters { get; set; }
        public int XpRandom { get; set; }
        public int Delay { get; set; }
        public string RolesPrefix { get; set; }
        public ulong LogChannelId { get; set; }
        public List<ulong> ChannelIds { get; set; } = new();

        public bool IsLevelUpChannel(ulong channelId)
        {
            return ChannelIds.Any(a => a == channelId);
        }

    }
}
