using Discord;
using Discord.WebSocket;
using Dynastio.Data;
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
        private readonly DynastioClient _dynastioClient;
        private readonly DynastioData _db;
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;

        public GuildService(IServiceProvider services)
        {
            Main.Log("Guild Service", "Start Async");

            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._db = services.GetRequiredService<DynastioData>();
            this._discord = services.GetRequiredService<DiscordSocketClient>();
            this._services = services;
        }

        public ulong GetChannelId(Channels.GuildChannelType t)
        {
            return Channels.ChannelIds[t];
        }
        public async Task<IUserMessage> SendMessageAsync(Channels.GuildChannelType _channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
        {
            var guild = _discord.GetGuild(Guilds.OfficialGuild);
            if (guild is null) return null;

            var channel = guild.GetTextChannel(Channels.ChannelIds[_channel]) ?? await _discord.GetChannelAsync(Channels.ChannelIds[_channel]) as ITextChannel;
            return await channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
        }
        private const ulong _rolesHeader = 1113080837303455794;
        public async Task<(ulong[] addedRoles, ulong[] removedRoles)> SyncUserBadges(User buser)
        {
            var officialGuild = await GetOfficialGuildAsync();
            if (officialGuild is null) return default;

            var guild = _discord.GetGuild(officialGuild.Id);
            if (guild is null) return default;

            var user = guild.GetUser(buser.Id);
            if (user is null) return default;

            List<Profile> profiles = new List<Profile>();
            foreach (var a in buser.Accounts)
            {
                try
                {
                    var p = await _dynastioClient.GetUserProfileAsync(a.Id);
                    profiles.Add(p);
                }
                catch { continue; }
            }
            var badges = profiles.SelectMany(a => a.Badges);
            var userRoles = user.Roles.Select(a => a.Id);

            var rolesToAdd = new List<ulong>();
            var rolesToRemove = new List<ulong>();
            foreach (var role in Global.Roles.BadgeRoles)
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

            if (badges.Count() > 0 && !user.Roles.Select(a => a.Id).Contains(_rolesHeader))
                rolesToAdd.Add(_rolesHeader);

            if (rolesToAdd.Count > 0)
                await user.AddRolesAsync(rolesToAdd);

            if (rolesToRemove.Count > 0)
                await user.RemoveRolesAsync(rolesToRemove);

            buser.last_badges_sync = DateTime.UtcNow;
            await _services.GetRequiredService<DynastioData>().UpdateAsync(buser);

            return (rolesToAdd.ToArray(), rolesToRemove.ToArray());
        }
        public Task<Guild> GetOfficialGuildAsync() => _db.GetGuildAsync(Guilds.OfficialGuild, false);
    }

}
