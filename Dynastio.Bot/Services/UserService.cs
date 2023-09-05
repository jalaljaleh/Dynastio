using Dynastio.Net;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Dynastio.Bot.Data;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Dynastio.Bot
{
    public class UserService
    {
        private readonly ConcurrentBag<User> _users;
        private readonly DynastioClient _dynastioClient;
        private readonly GuildService _guildService;
        private readonly IDynastioBotDatabase _db;
        private readonly IServiceProvider _services;
        public UserService(IServiceProvider services)
        {
            Global.Main.Log("User Service", "Start Async");

            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._db = services.GetRequiredService<IDynastioBotDatabase>();
            this._services = services;
            this._guildService = services.GetRequiredService<GuildService>();
            this._users = new();
        }

        public void ClearCache()
        {
            _users.Clear();
        }
        public async Task<bool> UpdateAsync(User user)
        {
            user.last_update = DateTime.UtcNow;
            return await _db.UpdateAsync(user);
        }
        private bool _isActivityLeaderboardCached = false;
        public async Task<List<User>> GetActivityScoreLeaderboardAsync(int count = 15)
        {
            if (_isActivityLeaderboardCached)
                return this._users
                    .OrderByDescending(a => a.activiy_level)
                    .ThenByDescending(a => a.activiy_score)
                    .Take(count)
                    .ToList();

            var users = await this._db.GetActivityScoreLeaderboardAsync(count);
            foreach (var user in users)
            {
                if (IsCached(user.Id) is false)
                    Cache(user);
            }
            _isActivityLeaderboardCached = true;

            return users;
        }

        private bool _isHonorLeaderboardCached = false;
        public async Task<List<User>> Get10TopHonor()
        {
            if (_isHonorLeaderboardCached)
                return this._users.OrderByDescending(a => a.Honor).Take(10).ToList();

            var users = await this._db.GetHonorLeaderboardAsync(10);
            foreach (var user in users)
            {
                if (IsCached(user.Id) is false)
                    Cache(user);
            }
            _isHonorLeaderboardCached = true;
            return users;
        }
        public async Task<User> GetUserAsync(ulong Id, bool New = true)
        {
            User user = _users.FirstOrDefault(x => x.Id == Id);
            if (user is null)
            {
                user = await _db.GetUserAsync(Id);

                if (user is null && New is true)
                {
                    user = await GetNewUserAsync(Id);

                    await _guildService.SyncUserBadges(user);

                    await _db.InsertAsync(user);
                }
                if (user != null)
                {
                    Cache(user);
                    
                    if ((DateTime.UtcNow - user.last_badges_sync).TotalDays > 3)
                        await _guildService.SyncUserBadges(user);
                }
            }
            return user;
        }
        async Task<User> GetNewUserAsync(ulong id)
        {
            var user = new User()
            {
                Id = id,
                Accounts = new(),
                Honor = 0,
                LastHonorGift = DateTime.MinValue,
                activiy_score = 0,
                activiy_level = 0,
                LastBoostGift = DateTime.MinValue,
                game_accountId = 0,
            };

            Profile profile;
            try { profile = await _dynastioClient.GetUserProfileAsync("discord:" + id); }
            catch
            {
                profile = null;
            }

            if (profile is not null)
            {
                var account = new UserAccount()
                {
                    Id = "discord:" + id,
                    AddedAt = DateTime.UtcNow,
                    PinCode = "none",
                    IsDefault = true,
                }.SetReminder("discord");
                user.Accounts.Add(account);
            }

            return user;
        }

        public async Task<User> GetUserByAccountIdAsync(string accountId)
        {
            User user = _users.FirstOrDefault(x => x.GetAccount(accountId) != null);
            if (user is null)
            {
                user = await _db.GetUserByAccountIdAsync(accountId);
                if (user != null)
                    Cache(user);
            }
            return user;
        }
        public void Cache(User user)
        {
            _users.Add(user);
        }
        public bool IsCached(ulong Id)
        {
            return this._users.FirstOrDefault(x => x.Id == Id) != null;
        }

    }
}
