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

namespace Dynastio.Data
{
    public enum BotUserPermission
    {
        CreateTicket
    }
    public class UserWarn
    {
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }
        public ulong SourceId { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class User
    {
        public User() { }
        public ulong Id { get; set; }
        public string youtube_channel { get; set; }
        public int activiy_level { get; set; }
        public long activiy_score { get; set; }
        public int activiy_score_additive { get; set; } = 0;
       
        public string gameAccountId { get; set; } = string.Empty;
        public List<BotUserPermission> LockedPermissions { get; set; } = new();
        public bool HasPermission(BotUserPermission per)
        {
            return !LockedPermissions.Contains(per);
        }
        public User LockPermission(BotUserPermission per)
        {
            if (!LockedPermissions.Contains(per))
                LockedPermissions.Add(per);
            return this;
        }
        public User UnlockPermission(BotUserPermission per)
        {
                LockedPermissions.Remove(per);
            return this;
        }
        public bool IsMainAccountConnected()
        {
            return !string.IsNullOrEmpty(this.gameAccountId);
        }
        public DateTime LastHonorGift { get; set; }
        public DateTime LastBoostGift { get; set; }
        public DateTime last_activiy_score_time { get; set; }
        public DateTime last_badges_sync { get; set; }

        [BsonIgnore]
        public DateTime last_update { get; set; }

        public List<UserWarn> Warns { get; set; } = new();

        public List<UserAccount> Accounts { get; set; } = new();

        public UserAccount GetAccount(string Id)
        {
            return Accounts.FirstOrDefault(a => a.Id.Equals(Id));
        }
        public UserAccount GetAccountByHashCode(string Id)
        {
            return Accounts.FirstOrDefault(a => a.GetHashCode().ToString().Equals(Id));
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

    //public class Inventory
    //{
    //    public ConcurrentDictionary<ItemType, long> Items { get; set; }
    //    public void AddItem(ItemType item, long count)
    //    {
    //        Items[item] += count;
    //    }
    //}
}
