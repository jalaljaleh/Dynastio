using Discord;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        static Dictionary<BadgeType, ulong> _roles = new Dictionary<BadgeType, ulong>()
        {
            { BadgeType.Developer, 1100680838678581269},
            { BadgeType.Monthly, 1098272223116148796},
            { BadgeType.CupBronze, 1106216604720701460},
            { BadgeType.CupPlatinum, 1106217232821915678},
            { BadgeType.CupSilver, 1106217385217753138},
            { BadgeType.Robot, 1101210991934586941},
            { BadgeType.MapMaker, 1106149689486757898 },
            { BadgeType.Friend, 1100739916410921000},
            { BadgeType.TopDonate, 1100687576324657182},
            { BadgeType.Premium, 1100484334500200511},
            { BadgeType.Supporter, 1100740908846153848},
            { BadgeType.Translator, 1100741214669647992},
            { BadgeType.YoutuberBronze, 1106217618416881664},
            { BadgeType.YoutuberGold, 1106217944054243449},
            { BadgeType.YoutuberPlatinum, 1106218053596872814},
            { BadgeType.YoutuberSilver, 1106218154365034606},
            { BadgeType.Void, 1100741981812051998},
        };
        public async Task SyncUserBadges(User buser)
        {
            var officialGuild = await GetOfficialGuildAsync();
            if (officialGuild is null) return;

            var guild = _discord.GetGuild(officialGuild.Id);
            if (guild is null) return;

            var user = guild.GetUser(buser.Id);
            if (user is null) return;

            List<Profile> profiles = new List<Profile>();
            foreach (var a in buser.Accounts)
            {
                try
                {
                    var p = await _dynastioClient.GetUserProfileAsync(a.Id);
                    profiles.Add(p);
                }
                catch
                {
                    continue;
                }
            }
            var badges = profiles.SelectMany(a => a.Badges);
            var userRoles = user.Roles.Select(a => a.Id);

            var rolesToAdd = new List<ulong>();
            var rolesToRemove = new List<ulong>();
            foreach (var role in _roles)
            {
                if (badges.Contains(role.Key) && !userRoles.Contains(role.Value))
                {
                    rolesToAdd.Add(role.Value);
                    continue;
                }
                if (!badges.Contains(role.Key) && userRoles.Contains(role.Value))
                {
                    rolesToRemove.Add(role.Value);
                }
            }
            if (rolesToAdd.Count > 0)
                await user.AddRolesAsync(rolesToAdd);

            if (rolesToRemove.Count > 0)
                await user.RemoveRolesAsync(rolesToRemove);
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

                    if (action != null)
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
