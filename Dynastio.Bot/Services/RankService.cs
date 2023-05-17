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
        private const int _nextScoreTime = 40;
        private const int _updateUserTime = 90;
        private int[] _randomScore = { 1, 15 };
        int getMax(int lvl)
        {
            return ((lvl + 20) * (int)Math.Pow(lvl, 2.1));
        }
        public async Task AddMemberRoles(IGuildUser duser, User buser)
        {
            var rankedRoles = duser.Guild.Roles
                .Where(x => x.Name.StartsWith("rank: "))
                .OrderBy(a => a.Position)
                .Select(a => a.Id)
                .ToList();

            var userRankedroles = duser.RoleIds.Where(a => rankedRoles.Contains(a));

            rankedRoles.AddRange(userRankedroles);

            var rolesToAdd = rankedRoles
                .GetRange(0, buser.activiy_level)
                .Distinct();

            await duser.AddRolesAsync(rolesToAdd);
        }
        public async Task AddMessageScoreAsync(IUserMessage message)
        {
            if (message.Channel is null ||
                message.Channel is not IGuildChannel ||
               !_score_channels.Contains(message.Channel.Id))
                return;



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

                    var max = getMax(_user.activiy_level);
                    if (_user.activiy_score > max)
                    {
                        _user.activiy_score = _user.activiy_score - max;
                        _user.activiy_level++;
                        try { await AddMemberRoles(message.Author as IGuildUser, _user); } catch { }
                    }
                    await _userService.UpdateAsync(_user);

                    user.Score = 0;
                }
            }
        }

        private ulong[] _score_channels = {
            480966712318099487, //
            486591124836974592, //
            1098632452274135112,//
            1098918867255967814,//
            1098248723013841026,//
            1098608343947415575,//
            1098263349873082438,//
        };
       
    }
    public class RankRecord
    {
        public ulong Id { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime LastUserUpdate { get; set; }
    }
}
