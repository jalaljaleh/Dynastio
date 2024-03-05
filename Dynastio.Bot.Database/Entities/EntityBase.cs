using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database.Entities
{
    public enum EntityType
    {
        Setting
    }
    public abstract class EntityBase
    {
        // ... existing base class properties

        public EntityType Type { get; set; } // Add the discriminator field
    }
    public class BotConfiguration : EntityBase
    {
        public BotConfiguration()
        {
       
        }

    }
 
}
