using Dynastio.Bot.Database.Entities;
using Dynastio.Bot.Global;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dynastio.Bot.Database.RedeemCode;
using static System.Collections.Specialized.BitVector32;

namespace Dynastio.Bot.Database
{
    public class DynastioBotDatabase
    {
        private IDynastioDatabase dbContext;
        private readonly ConcurrentBag<User> _users;
        private readonly ConcurrentBag<Guild> _guilds;

        public DynastioBotDatabase()
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

        public async Task<Advertise> GetAdAsync(ObjectId Id)
        {
            return await dbContext.GetAdvertisingAsync(Id);
        }
        public async Task<bool> InsertAsync(Advertise _advertise)
        {
            return await dbContext.InsertAsync(_advertise);
        }
        public async Task<bool> UpdateAsync(Advertise _advertise)
        {
            return await dbContext.UpdateAsync(_advertise);
        }
        public async Task<bool> UpdateManyAsync(List<Advertise> _advertise)
        {
            return await dbContext.UpdateManyAsync(_advertise);
        }
        public async Task<List<Advertise>> GetAvailableAdsAsync()
        {
            return await dbContext.GetAdsAsync(x => x.DisplayCount < x.Count);
        }


        public async Task<T> GetEntityAsync<T>(EntityType entity) where T : EntityBase
        {
            return await dbContext.GetEntityAsync<T>(entity);
        }
        public async Task<bool> InsertAsync<T>(T entity) where T : EntityBase
        {
            return await dbContext.InsertAsync<T>(entity);
        }
        public async Task<bool> UpdateAsync<T>(T entity) where T : EntityBase
        {
            return await dbContext.UpdateAsync<T>(entity);
        }

        public async Task<List<Guild>> GetGuildsWithoutSubscriptionAsync()
        {
            return await dbContext.GetGuildsAsync(a => a.Subscription.EndsAt > DateTime.UtcNow);
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
            return await dbContext.UpdateAsync(user);
        }
        private void Cache(User user)
        {
            _users.Add(user);
        }

    }
}
