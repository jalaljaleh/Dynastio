using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class GuildServices
    {
        private readonly DynastioBotDatabase _db;
        /// <summary>
        /// Constructor with required dependencies injected.
        /// </summary>
        public GuildServices(DynastioBotDatabase db)
        {
            _db = db;

        }


        /// <summary>
        /// Fetches an existing guild or creates a new default record.
        /// </summary>
        public async Task<Guild> GetOrCreateGuildAsync(ulong botGuildId, bool alloCreate = true)
        {
            return await _db.GetGuildAsync(botGuildId, allowCreate: alloCreate);
        }
        /// <summary>
        /// Updates only the LastUpdateTime on a guild record.
        /// </summary>
        public async Task<bool> UpdateGuildAsync(Guild guild)
        {
            guild.Touch();
            return await _db.UpdateGuildAsync(guild);
        }
    }
}
