using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;

namespace Dynastio.Bot.Data
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public User() { }
        public ulong Id { get; set; }
        public int Honor { get; set; }
        public DateTime LastHonorGift { get; set; }

        public List<UserAccount> Accounts { get; set; } = new();
        public UserAccount GetAccount(string Id)
        {
            return Accounts.FirstOrDefault(a => a.Id.Equals(Id));
        }
        public UserAccount GetDefaultAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsDefault);
        }
    }


}
