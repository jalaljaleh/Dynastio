using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    public class Channel
    {

    }
    [BsonIgnoreExtraElements]
    public class Guild
    {
        public Guild() { }
        public ulong Id { get; set; }
        //public Dictionary<BotGuildRoleType, ulong> Roles { get; set; } = new();
        //public Dictionary<BotGuildChannelType, ulong> Channels { get; set; } = new();
        public bool IsOfficialServer { get; set; } = false;
        //public ulong GetRoleId(BotGuildRoleType role)
        //{
        //    if (Roles.TryGetValue(role, out ulong id))
        //        return id;
        //    return 0;
        //}
        //public void AddOrUpdateRoleId(BotGuildRoleType role, ulong Id)
        //{
        //    if (Roles.ContainsKey(role))
        //        Roles[role] = Id;
        //    else
        //        Roles.Add(role, Id);

        //}
        //public void AddOrUpdateChannelId(BotGuildChannelType channel, ulong Id)
        //{
        //    if (Channels.ContainsKey(channel))
        //        Channels[channel] = Id;
        //    else
        //        Channels.Add(channel, Id);
        //}
        //public ulong GetChannelId(BotGuildChannelType channel)
        //{
        //    if (Channels.TryGetValue(channel, out ulong id))
        //        return id;
        //    return 0;
        //}
        public enum BotGuildRoleType
        {
            // Dynastio Roles
            Developer,
            Supporter,
            Administrator = 3,
            Monthly,
            Friend,
            Translator,
            YoutuberBronze = 2,
            YoutuberSilver = 7,
            YoutuberGold,
            YoutuberPlatinum,
            CupBronze,
            CupSilver,
            CupGold,
            CupPlatinum,
            Void,
            SuperDeveloper,
            Premium,
            MapMaker,
            TopDonate,
            Robot,
        }
        public enum BotGuildChannelType
        {
            DeletedMessages,
            Moderation,
            UserJoined,
            UserLeave,
            Giveaway,
            ServerBoosts
        }
    }


}


