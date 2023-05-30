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
        public const ulong _officialGuildId = 480416088312774657;

        readonly static Dictionary<BadgeType, ulong> _roles = new()
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
        readonly static Dictionary<GuildChannelType, ulong> _channels = new()
        {
            { GuildChannelType.None, 0},
            { GuildChannelType.TopActive, 480966712318099487},
            { GuildChannelType.LoggerChannel, 1107754629200089180},
            { GuildChannelType.General, 480966712318099487},
            { GuildChannelType.MemberChannel, 1109020050163240990},
        };
        readonly static Dictionary<ChannelThreadType, ulong> _threads = new()
        {
            { ChannelThreadType.None, 0},
            { ChannelThreadType.DeletedMessages, 1107764609848463460 },
            { ChannelThreadType.EditedMessages, 1107930813569454160 },
            { ChannelThreadType.TimeOut, 1112059604071874640 },
        };
        public enum GuildChannelType
        {
            None,
            TopActive,
            LoggerChannel,
            General,
            MemberChannel
        }
        public enum ChannelThreadType
        {
            None,
            DeletedMessages,
            EditedMessages,
            TimeOut
        }
        public ulong GetChanneThreadlId(ChannelThreadType t)
        {
            return _threads[t];
        }
        public ulong GetChannelId(GuildChannelType t)
        {
            return _channels[t];
        }
        public async Task<IUserMessage> SendMessageAsync(GuildChannelType _channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
        {
            var guild = _discord.GetGuild(_officialGuildId);
            if (guild is null) return null;

            var channel = guild.GetTextChannel(_channels[_channel]) ?? await _discord.GetChannelAsync(_channels[_channel]) as ITextChannel;
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

            if (badges.Count() > 0 && !user.Roles.Select(a=>a.Id).Contains(_rolesHeader))
                rolesToAdd.Add(_rolesHeader);

            if (rolesToAdd.Count > 0)
                await user.AddRolesAsync(rolesToAdd);

            if (rolesToRemove.Count > 0)
                await user.RemoveRolesAsync(rolesToRemove);
            
            buser.last_badges_sync = DateTime.UtcNow;
            await _services.GetRequiredService<UserService>().UpdateAsync(buser);

            return (rolesToAdd.ToArray(), rolesToRemove.ToArray());
        }
        public async Task<Guild> GetOfficialGuildAsync() => await GetGuildAsync(_officialGuildId, false);

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
