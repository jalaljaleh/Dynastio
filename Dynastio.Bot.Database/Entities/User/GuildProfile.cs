using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    public class GuildProfile
    {
        public ulong GuildId { get; set; }

        public long Xp { get; set; } = 0;
        public int Level { get; set; } = 0;
        public int Warns { get; set; }
        public DateTime LastMessageTimestamp { get; set; } = DateTime.MinValue;

        public DateTime LastCheckupTimestamp { get; set; }
    }
}
