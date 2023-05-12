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
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _discord;
        private readonly UserService _userService;
        private readonly IDynastioBotDatabase _database;
        public RankService(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _userService = services.GetRequiredService<UserService>();
            _database = services.GetRequiredService<IDynastioBotDatabase>();
        }

        ConcurrentBag<RankRecord> _temporaryHolder = new();

        ulong[] _score_channels = {
            480966712318099487,
            486591124836974592,
            1098632452274135112,
            1098918867255967814,
            1098248723013841026,
            1098608343947415575,
            1098263349873082438,
        };
        ulong _active_role = 1098350344368558200;
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
                            await user.AddRoleAsync(_active_role);
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
        private DateTime _lastSyncTime = DateTime.MinValue;
        public async Task AddMessageScoreAsync(IUserMessage message)
        {
            if (message.Channel is null ||
                message.Channel is not IGuildChannel ||
               !_score_channels.Contains(message.Channel.Id))
                return;

            if ((DateTime.UtcNow - _lastSyncTime).TotalMinutes > 20)
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

            if ((DateTime.UtcNow - user.LastUpdate).TotalMinutes > 1)
            {
                user.Score = user.Score + Global.Main.Random.Next(0, 15);
                user.LastUpdate = DateTime.UtcNow;
            }
            else return;

            if ((DateTime.UtcNow - user.LastUserUpdate).TotalMinutes > 10)
            {
                user.LastUserUpdate = DateTime.UtcNow;

                var _user = await _userService.GetUserAsync(userId);
                _user.activiy_score += user.Score;

                await _userService.UpdateAsync(_user);

                user.Score = 0;
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
