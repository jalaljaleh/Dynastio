using Dynastio.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class GuildService
    {
        private readonly ConcurrentBag<Guild> guilds;
        private readonly IDatabaseContext db;
        public GuildService(IDatabaseContext mongo)
        {
            Program.Log("GuildService", "StartAsync");

            guilds = new();
            this.db = mongo;
        }
        public async Task<Guild> GetGuildAsync(ulong id)
        {
            Guild guild = guilds.FirstOrDefault(a => a.Id == id);
            if (guild == null || guild == default)
            {
                guild = await db.GetGuildAsync(id);
                if (guild == null || guild == default)
                {
                    guild = new Guild()
                    {
                        Id = id,
                    };
                    await db.InsertAsync(guild);
                }
                guilds.Add(guild);
            }
            return guild;
        }
        public async Task<bool> UpdateAsync(Guild guild)
        {
            await db.UpdateAsync(guild);
            return true;
        }
    }
}
