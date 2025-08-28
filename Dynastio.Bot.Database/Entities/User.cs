using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Dynastio.Bot.Database
{

    [BsonIgnoreExtraElements]
    public class User
    {
        public User() { }
        [BsonId]
        public ulong Id { get; set; }
        public string youtube_channel { get; set; }
        public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
        [JsonIgnore]
        public string gameAccountId { get; set; } = string.Empty;
        public List<UserGameAccount> Accounts { get; set; } = new();
        public List<UserGuildProfile> GuildProfiles { get; set; } = new();


        [BsonIgnore]
        public bool IsAccountConnected { get => Accounts.Any(); }


        public UserGuildProfile GetGuildProfile(ulong guildId)
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
                    Xp = 0,
                };
                GuildProfiles.Add(rankingProfile);
            }
            return rankingProfile;
        }

        public UserGameAccount GetAccount(string Id)
        {
            return Accounts.FirstOrDefault(a => a.Id.Equals(Id));
        }
        public UserGameAccount GetAccountByHashCode(string Id, out UserGameAccount userAccount)
        {
            userAccount = Accounts.FirstOrDefault(a => a.GetHashCode().ToString().Equals(Id));
            return userAccount;
        }
        public UserGameAccount GetDefaultAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsDefault) ?? Accounts.FirstOrDefault();
        }

        public void SwitchAccount(ref UserGameAccount userAccount)
        {
            Accounts.ForEach(a => a.IsDefault = false);
            userAccount.IsDefault = true;
        }
        public string GetAccountService() => gameAccountId.Split(":")[0];

      
    }


}
