using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class XpSystemSettings
    {
        public XpSystemSettings()
        {
        }

        public bool IsEnabled { get; set; }
        public bool IsGameRewardEnabled { get; set; }
        public bool IsRankingRoleAssignmentEnabled { get; set; }

      //  public ulong RankingRoleAssignmentHeaderId { get; set; }
        public string RankingRoleAssignmentPerfix { get; set; }

        public int XpPerMessage { get; set; }
        public int XpBoosters { get; set; }
        public int XpRandom { get; set; }

        public int MessageScoreCooldown { get; set; }

        public ulong RankingLoggerChannelId { get; set; }

        public List<ulong> AllowedXpChannels { get; set; } = new();

        public bool IsXpChannel(ulong channelId)
        {
            return AllowedXpChannels.Any(a => a == channelId);
        }

    }
}
