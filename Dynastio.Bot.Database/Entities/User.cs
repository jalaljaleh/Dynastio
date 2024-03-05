using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using System.Collections.Concurrent;
using Dynastio.Net;
using Dynastio.Bot.Global;

namespace Dynastio.Bot.Database
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public User() { }
        public ulong Id { get; set; }

        public string youtube_channel { get; set; }

        public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;

        public string gameAccountId { get; set; } = string.Empty;
        public List<UserAccount> Accounts { get; set; } = new();
        public List<GuildProfile> GuildProfiles { get; set; } = new();
        public GuildProfile GetRankingProfile(ulong guildId)
        {
            var rankingProfile = GuildProfiles.FirstOrDefault(a => a.GuildId == guildId);
            if (rankingProfile is null)
            {
                rankingProfile = new()
                {
                    GuildId = guildId,
                    LastCheckupTimestamp = DateTime.MinValue,
                    LastMessageTimestamp = DateTime.MinValue,
                    Level = 0,
                    Xp = 0
                };
                GuildProfiles.Add(rankingProfile);
            }
            return rankingProfile;
        }
        public UserAccount GetAccount(string Id)
        {
            return Accounts.FirstOrDefault(a => a.Id.Equals(Id));
        }
        public UserAccount GetAccountByHashCode(string Id, out UserAccount userAccount)
        {
            userAccount = Accounts.FirstOrDefault(a => a.GetHashCode().ToString().Equals(Id));
            return userAccount;
        }
        public UserAccount GetDefaultAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsDefault) ?? Accounts.FirstOrDefault();
        }
        public void SwitchAccount(ref UserAccount userAccount)
        {
            Accounts.ForEach(a => a.IsDefault = false);
            userAccount.IsDefault = true;
        }
    }


}
