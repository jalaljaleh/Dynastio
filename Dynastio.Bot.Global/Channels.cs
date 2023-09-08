using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class Channels
    {
        public enum GuildChannelType
        {
            None,
            TopActive,
            DeletedMessages,
            EditedMessages,
            TimeOut,
            General,
            MemberChannel,
            RewardChannel,
            ConfirmPromoteVideos
        }
        public readonly static Dictionary<GuildChannelType, ulong> ChannelIds = new()
        {
            { GuildChannelType.None, 0},
            { GuildChannelType.TopActive, 480966712318099487},
            { GuildChannelType.DeletedMessages, 1120683501256188005},
            { GuildChannelType.TimeOut, 1120683687210651678},
            { GuildChannelType.EditedMessages, 1120683592566190080},
            { GuildChannelType.General, 480966712318099487},
            { GuildChannelType.MemberChannel, 1109020050163240990},
            { GuildChannelType.RewardChannel, 1108998382996946964},
            { GuildChannelType.ConfirmPromoteVideos, 1147964955547668600},
        };
    }
}
