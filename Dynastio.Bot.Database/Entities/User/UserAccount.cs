using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynastio.Bot.Database
{

    [BsonIgnoreExtraElements]
    public class UserAccount
    {
        public UserAccount() { }
        public UserAccount(string Id) { this.Id = Id; }

        [BsonId]
        public string Id { get; set; }


        [BsonDefaultValue("none")]
        public string Reminder { get; internal set; }

        public UserAccount SetReminder(string text)
        {
            Reminder = string.IsNullOrEmpty(text)
                ? Id.Split(":")[0]
                : text.TryRemove(16).Trim(); ;
            return this;
        }
       
        public DateTime AddedAt { get; set; }

        public bool IsDefault { get; set; } = false;

        public string PinCode { get; set; }
        public string Email { get; set; } = "none";
        public string GetAccountService() => Id.Split(":")[0];
        public static string GetAccountService(string Id) => Id.Split(":")[0];
        public static string GetAccountId(string Id) => Id.Split(":")[1];

    }

}
