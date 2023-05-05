using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    [BsonIgnoreExtraElements]
    public class Guild
    {
        public Guild() { }
        public ulong Id { get; set; }
        public Dictionary<BotGuildRoleType, ulong> Roles { get; set; } = new();
        public bool IsOfficialServer { get; set; } = false;
        public ulong GetRoleId(BotGuildRoleType role)
        {
            if (Roles.TryGetValue(role, out ulong id))
            {
                return id;
            }
            return 0;
        }
        public void AddOrUpdateRoleId(BotGuildRoleType role, ulong Id)
        {
            if (Roles.ContainsKey(role))
            {
                Roles[role] = Id;
            }
            else
            {
                Roles.Add(role, Id);
            }
        }
        public enum BotGuildRoleType
        {
            Developer
        }
        public enum LoggerChannelType
        {
            DeletedMessages,
            Moderation,
            UserJoined,
            UserLeave,
            Giveaway
        }
    }
  

}


