using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using System.Collections.Concurrent;
using Dynastio.Net;

namespace Dynastio.Bot.Data
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public User() { }
        public ulong Id { get; set; }
        public int Honor { get; set; }

        public int activiy_level { get; set; }
        public long activiy_score { get; set; }


        public DateTime LastHonorGift { get; set; }
        public DateTime LastBoostGift { get; set; }
        public DateTime last_activiy_score_time { get; set; }
        public DateTime last_badges_sync { get; set; }

        [BsonIgnore]
        public DateTime last_update { get; set; }

        //public Inventory Inventory { get; set; }
        public List<UserAccount> Accounts { get; set; } = new();

        public UserAccount GetAccount(string Id)
        {
            return Accounts.FirstOrDefault(a => a.Id.Equals(Id));
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
