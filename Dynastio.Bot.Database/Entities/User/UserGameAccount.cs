using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{

    [BsonIgnoreExtraElements]
    public class UserGameAccount
    {
        public UserGameAccount() { }
        public UserGameAccount(string Id) { this.Id = Id; }

        [BsonId]
        public string Id { get; set; }
        public string PinCode { get; set; }
        public string Email { get; set; } = "none";
        public bool IsDefault { get; set; } = false;
        public DateTime AddedAt { get; set; }

        [BsonDefaultValue("none")]
        public string Reminder { get; internal set; }


        public string GetAccountService() => Id.Split(":")[0];
        public static string GetAccountId(string Id) => Id.Split(":")[1];
        public UserGameAccount SetReminder(string text)
        {
            Reminder = string.IsNullOrEmpty(text)
                ? Id.Split(":")[0]
                : text.TryRemove(16).Trim(); ;
            return this;
        }
    }

}
