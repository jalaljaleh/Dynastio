using Dynastio.Bot.Global;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dynastio.Data.RedeemCode;
using static System.Collections.Specialized.BitVector32;

namespace Dynastio.Data
{
    public class DynastioData
    {
        private IDynastioDatabase dbContext;
        private readonly ConcurrentBag<User> _users;
        private readonly ConcurrentBag<Guild> _guilds;

        public DynastioData()
        {

            this._users = new();
            this._guilds = new();
        }
        public enum DatabasesInstances { Mongodb }
        public async Task InitializeAsync(string connection, DatabasesInstances instances = DatabasesInstances.Mongodb)
        {
            if (instances is DatabasesInstances.Mongodb)
            {
                dbContext = new MongoDbContext(connection);
                await dbContext.InitializeAsync();
                return;
            }
            dbContext = await Task.FromResult<IDynastioDatabase>(null);
        }

        public async Task<Guild> GetGuildAsync(ulong Id, bool New = true, Action<Guild> action = null)
        {
            Guild guild = _guilds.FirstOrDefault(a => a.Id == Id);
            if (guild == null || guild == default)
            {
                guild = await dbContext.GetGuildAsync(Id);
                if (guild == null || guild == default && New)
                {
                    guild = new Guild()
                    {
                        Id = Id,
                    };

                    if (action != null)
                        action.Invoke(guild);

                    await dbContext.InsertAsync(guild);
                }
                if (guild is not null)
                    _guilds.Add(guild);
            }
            return guild;
        }
        public async Task<bool> UpdateAsync(Guild guild)
        {
            return await dbContext.UpdateAsync(guild);
        }
        public async Task<bool> InsertAsync(Guild guild)
        {
            return await dbContext.InsertAsync(guild);
        }

        private List<RedeemCode> _redeemcodes;
        private bool isRedeemcodesCached = false;
        public async Task<RedeemCode> GetRedeemCodeAsync(RedeemType type)
        {
            var list = await GetRedeemCodesAsync();
            return list.FirstOrDefault(x => x.Type == type);
        }
        public async Task<List<RedeemCode>> GetRedeemCodesAsync()
        {
            if (isRedeemcodesCached is false)
            {
                _redeemcodes = await dbContext.GetRedeemCodesAsync();
                isRedeemcodesCached = true;
            }
            return _redeemcodes;
        }
        public async Task<bool> InsertAsync(RedeemCode redeemCodes)
        {
            var list = await GetRedeemCodesAsync();
            list.Add(redeemCodes);
            return await dbContext.InsertAsync(redeemCodes);
        }
        public async Task<bool> InsertManyAsync(List<RedeemCode> redeemCodes)
        {
            var list = await GetRedeemCodesAsync();
            list.AddRange(redeemCodes);
            return await dbContext.InsertManyAsync(redeemCodes);
        }
        public async Task<bool> DeleteAsync(RedeemCode redeemCodes)
        {
            var list = await GetRedeemCodesAsync();
            list.Remove(redeemCodes);
            return await dbContext.DeleteAsync(redeemCodes);
        }

        public async Task<User> GetUserAsync(ulong Id, bool New = true)
        {
            User user = _users.FirstOrDefault(x => x.Id == Id);
            if (user is null)
            {
                user = await dbContext.GetUserAsync(Id);

                if (user is null && New is true)
                {
                    user = new()
                    {
                        Id = Id,
                    };
                    await dbContext.InsertAsync(user);
                }
                if (user != null)
                {
                    Cache(user);
                }
            }
            return user;
        }
        public async Task<User> GetUserByYoutubeChannelIdAsync(string channelurl)
        {
            User user = _users.FirstOrDefault(x => x.youtube_channel == channelurl);
            if (user is null)
            {
                user = await dbContext.GetUserByYoutubeChannelIdAsync(channelurl);
                if (user != null)
                    Cache(user);
            }
            return user;
        }
        public async Task<User> GetUserByAccountIdAsync(string accountId)
        {
            User user = _users.FirstOrDefault(x => x.GetAccount(accountId) != null);
            if (user is null)
            {
                user = await dbContext.GetUserByAccountIdAsync(accountId);
                if (user != null)
                    Cache(user);
            }
            return user;
        }
        public async Task<User> GetUserByConnectedAccountIdAsync(string accountId)
        {
            User user = _users.FirstOrDefault(x => x.gameAccountId == accountId);
            if (user is null)
            {
                user = await dbContext.GetUserByConnectedAccountIdAsync(accountId);
                if (user != null)
                    Cache(user);
            }
            return user;
        }
        public async Task<bool> UpdateAsync(User user)
        {
            user.last_update = DateTime.UtcNow;
            return await dbContext.UpdateAsync(user);
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

            var users = await this.dbContext.GetActivityScoreLeaderboardAsync(count);
            foreach (var user in users)
            {
                if (IsCached(user.Id) is false)
                    Cache(user);
            }
            _isActivityLeaderboardCached = true;
            return users;
        }
        private void Cache(User user)
        {
            _users.Add(user);
        }
        public bool IsCached(ulong Id)
        {
            return this._users.Any(x => x.Id == Id);
        }

    }
}
