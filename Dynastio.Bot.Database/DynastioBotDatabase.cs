using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task InitializeAsync(string connection, DatabasesInstances instances = DatabasesInstances.Mongodb, bool isDebugMode = false)
        {
            if (instances is DatabasesInstances.Mongodb)
            {
                dbContext = new MongoDbContext(connection, isDebugMode);
                await dbContext.InitializeAsync();
                return;
            }
            dbContext = await Task.FromResult<IDynastioDatabase>(null);
        }
        public void ClearCache()
        {
            this._users.Clear();
            this._guilds.Clear();
        }

       
        public async Task<Guild> GetGuildAsync(ulong id, bool allowCreate = true, Func<Guild> createInstance = null, Action<Guild> onCreate = null)
        {
            var guild = _guilds.FirstOrDefault(g => g.Id == id);
            if (guild is not null)
                return guild;

            guild = await dbContext.GetGuildAsync(id).ConfigureAwait(false);
            if (guild is not null)
            {
                _guilds.Add(guild);
                return guild;
            }

            if (!allowCreate)
                return null;

            guild = createInstance?.Invoke() ?? new Guild { Id = id };

            onCreate?.Invoke(guild);

            await dbContext.InsertAsync(guild).ConfigureAwait(false);
            _guilds.Add(guild);

            return guild;
        }
        //public async Task<Guild> GetGuildAsync(ulong Id, bool New = true, Func<Guild> createInstanseFunc = null, Action<Guild> onCreate = null)
        //{
        //    Guild guild = _guilds.FirstOrDefault(a => a.Id == Id);
        //    if (guild == null || guild == default)
        //    {
        //        guild = await dbContext.GetGuildAsync(Id);
        //        if (guild == null || guild == default && New)
        //        {
        //            if (createInstanseFunc is not null)
        //                guild = createInstanseFunc.Invoke();
        //            else
        //                guild = new Guild()
        //                {
        //                    Id = Id,

        //                };

        //            if (onCreate != null)
        //                onCreate.Invoke(guild);

        //            await dbContext.InsertAsync(guild);
        //        }
        //        if (guild is not null)
        //            _guilds.Add(guild);
        //    }
        //    return guild;
        //}
        public async Task<bool> UpdateAsync(Guild guild)
        {
            return await dbContext.UpdateAsync(guild);
        }
        public async Task<bool> InsertAsync(Guild guild)
        {
            return await dbContext.InsertAsync(guild);
        }

        public async Task<User?> GetUserAsync(ulong id, bool allowCreate = true, Func<User>? createInstance = null, Action<User>? onCreate = null)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is not null)
                return user;

            user = await dbContext.GetUserAsync(id).ConfigureAwait(false);
            if (user is not null)
            {
                Cache(user);
                return user;
            }

            if (!allowCreate)
                return null;

            user = createInstance?.Invoke() ?? new User { Id = id };

            onCreate?.Invoke(user);

            await dbContext.InsertAsync(user).ConfigureAwait(false);
            Cache(user);

            return user;
        }

        //public async Task<User> GetUserAsync(ulong Id, bool New = true, Func<User>? createInstance = null, Action<User> actionOnNew = null)
        //{
        //    User user = _users.FirstOrDefault(x => x.Id == Id);
        //    if (user is null)
        //    {
        //        user = await dbContext.GetUserAsync(Id);

        //        if (user is null && New is true)
        //        {

        //            if (createInstance is not null)
        //                user = createInstance.Invoke();
        //            else
        //                user = new()
        //            {
        //                Id = Id,
        //            };

        //            if (actionOnNew != null)
        //                actionOnNew.Invoke(user);

        //            await dbContext.InsertAsync(user);
        //        }
        //        if (user != null)
        //        {
        //            Cache(user);
        //        }
        //    }
        //    return user;
        //}
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
