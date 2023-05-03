using Dynastio.Bot.Data;
using Dynastio.Bot.Global;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ConcurrentBag<Guild> _guilds;
        private readonly DynastioClient _dynastioClient;
        private readonly IDynastioBotDatabase _db;
        private readonly IServiceProvider _services;
        public GuildService(IServiceProvider services)
        {
            Main.Log("Guild Service", "Start Async");

            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._db = services.GetRequiredService<IDynastioBotDatabase>();
            this._services = services;

            this._guilds = new();
        }
        public async Task<Guild> GetGuildAsync(ulong id)
        {
            Guild guild = _guilds.FirstOrDefault(a => a.Id == id);
            if (guild == null || guild == default)
            {
                guild = await _db.GetGuildAsync(id);
                if (guild == null || guild == default)
                {
                    guild = new Guild()
                    {
                        Id = id,
                    };
                    await _db.InsertAsync(guild);
                }
                _guilds.Add(guild);
            }
            return guild;
        }
        public async Task<bool> UpdateAsync(Guild guild)
        {
            await _db.UpdateAsync(guild);
            return true;
        }
    }
}
