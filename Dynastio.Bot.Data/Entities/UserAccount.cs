using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynastio.Bot.Data
{

    [BsonIgnoreExtraElements]
    public class UserAccount
    {
        public UserAccount() { }
        public UserAccount(string Id) { this.Id = Id; }

        [BsonId]
        public string Id { get; set; }

        public string Nickname { get; set; }

        [BsonDefaultValue("none")]
        public string Reminder { get; set; } = "none";

        public DateTime AddedAt { get; set; }

        public bool IsDefault { get; set; } = false;


        public string GetAccountService() => Id.Split(":")[0];

    }

}
