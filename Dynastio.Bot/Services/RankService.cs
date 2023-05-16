using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class RankService
    {
        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly DiscordSocketClient _discord;
        private readonly IDynastioBotDatabase _database;
        private readonly IServiceProvider _services;
        public RankService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _userService = services.GetRequiredService<UserService>();
            _guildService = services.GetRequiredService<GuildService>();
            _database = services.GetRequiredService<IDynastioBotDatabase>();
            _services = services;
        }

        ConcurrentBag<RankRecord> _temporaryHolder = new();
        private const int _syncRolesTime = 90;
        private const int _nextScoreTime = 70;
        private const int _updateUserTime = 90;
        private int[] _randomScore = { 0, 15 };
        public async Task AddMessageScoreAsync(IUserMessage message)
        {
            if (message.Channel is null ||
                message.Channel is not IGuildChannel ||
               !_score_channels.Contains(message.Channel.Id))
                return;

            if ((DateTime.UtcNow - _lastSyncTime).TotalSeconds > _syncRolesTime)
            {
                _lastSyncTime = DateTime.UtcNow;
                await SyncRankRoles();
            }

            var userId = message.Author.Id;

            var user = _temporaryHolder.FirstOrDefault(x => x.Id == userId);
            if (user is null)
            {
                user = new RankRecord()
                {
                    Id = userId,
                    LastUpdate = DateTime.MinValue,
                    Score = 0,
                    LastUserUpdate = DateTime.MinValue,
                };
                _temporaryHolder.Add(user);
            }
            //increase score
            if ((DateTime.UtcNow - user.LastUpdate).TotalSeconds > _nextScoreTime)
            {
                user.Score = user.Score + Global.Main.Random.Next(_randomScore[0], _randomScore[1]);
                user.LastUpdate = DateTime.UtcNow;

                // save user score
                if ((DateTime.UtcNow - user.LastUserUpdate).TotalSeconds > _updateUserTime)
                {
                    user.LastUserUpdate = DateTime.UtcNow;

                    var _user = await _userService.GetUserAsync(userId);
                    _user.activiy_score += user.Score;

                    await _userService.UpdateAsync(_user);

                    user.Score = 0;
                }
            }
        }

        private DateTime _lastSyncTime = DateTime.MinValue;
        private ulong[] _score_channels = {
            480966712318099487, //
            486591124836974592, //
            1098632452274135112,//
            1098918867255967814,//
            1098248723013841026,//
            1098608343947415575,//
            1098263349873082438,//
        };
        private const ulong _active_role = 1098350344368558200;
        public async Task SyncRankRoles()
        {
            var guild = _discord.GetGuild(GuildService._officialGuildId);
            if (guild is null) return;

            var role = guild.GetRole(_active_role);
            var roleMembers = role.Members.Select(a => a.Id);

            var _topMembers = await _userService.GetActivityScoreLeaderboardAsync(10);
            var topMembers = _topMembers.Select(a => a.Id);

            foreach (var t in topMembers)
            {
                try
                {
                    if (!roleMembers.Contains(t))
                    {
                        var user = guild.GetUser(t);
                        if (user is not null)
                        {
                            await user.AddRoleAsync(_active_role);
                            await _guildService.SendMessageAsync(GuildChannelType.TopActive,
                                text: user.Id.ToUserMention(),
                                embed: new EmbedBuilder()
                                {
                                    Title = "rankservice.role.added.title",
                                    Description = "rankservice.role.added.description",
                                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/1098332386674085988/1107719615678791781/circle_of_sacrifices_glow.png"
                                }.Build());
                        }
                    }
                }
                catch { }
                await Task.Delay(150);
            }
            foreach (var m in roleMembers)
            {
                try
                {
                    if (!topMembers.Contains(m))
                    {
                        var user = guild.GetUser(m);
                        if (user is not null)
                            await user.RemoveRoleAsync(_active_role);
                    }
                }
                catch { }
                await Task.Delay(150);
            }
        }
    }
    public class RankRecord
    {
        public ulong Id { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime LastUserUpdate { get; set; }
    }
}
