using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Global;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
        private readonly DiscordSocketClient _discord;
        public GuildService(IServiceProvider services)
        {
            Main.Log("Guild Service", "Start Async");

            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._db = services.GetRequiredService<IDynastioBotDatabase>();
            this._discord = services.GetRequiredService<DiscordSocketClient>();
            this._services = services;

            this._guilds = new();
        }
        public async Task SetOfficialGuildAsync(ulong Id)
        {
            if (!_discord.Guilds.Any(a => a.Id == Id))
                throw new Exception("oldOfficialGuild.unavailable");

            var oldOfficialGuild = await GetOfficialGuildAsync();
            if (oldOfficialGuild is not null)
            {
                await UpdateAsync(oldOfficialGuild, x => x.IsOfficialServer = false);
            }

            var newOfficialGuild = await GetGuildAsync(Id, false);

            if (newOfficialGuild is not null)
                await UpdateAsync(newOfficialGuild, x => x.IsOfficialServer = true);

            newOfficialGuild = await GetGuildAsync(Id, true, x => x.IsOfficialServer = true);
        }
        public async Task<Guild> GetOfficialGuildAsync()
        {
            var oldOfficialGuild = _guilds.FirstOrDefault(a => a.IsOfficialServer);
            if (oldOfficialGuild is null) //check db
            {
                oldOfficialGuild = await _db.GetOfficialGuildAsync();
            }
            return oldOfficialGuild;
        }

        public async Task<Guild> GetGuildAsync(ulong id, bool New = true, Action<Guild> action = null)
        {
            Guild guild = _guilds.FirstOrDefault(a => a.Id == id);
            if (guild == null || guild == default)
            {
                guild = await _db.GetGuildAsync(id);
                if (guild == null || guild == default && New)
                {
                    guild = new Guild()
                    {
                        Id = id,
                        IsOfficialServer = false,
                    };
                    action.Invoke(guild);
                    await _db.InsertAsync(guild);
                }
                if (guild is not null)
                    _guilds.Add(guild);
            }
            return guild;
        }
        public async Task<bool> UpdateAsync(Guild guild, Action<Guild> action = null)
        {
            if (action != null)
                action.Invoke(guild);

            await _db.UpdateAsync(guild);
            return true;
        }
    }
}
