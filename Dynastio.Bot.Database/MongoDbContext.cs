using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Provides MongoDB context for interacting with Dynastio database collections.
    /// </summary>
    internal class MongoDbContext : IDynastioDatabase
    {
        private MongoClient _mongoClient { get; set; }
        private IMongoDatabase _database;

        // MongoDB collections
        private IMongoCollection<User> _users => _database.GetCollection<User>("Users");
        private IMongoCollection<Guild> _guilds => _database.GetCollection<Guild>("Guilds");

        // Event for logging DB messages
        public event OnDatabaseMessageLogs OnMessagesLog;
        public delegate void OnDatabaseMessageLogs(string serviceName, string message, ConsoleColor color = default);

        /// <summary>
        /// Initializes a new MongoDB context.
        /// </summary>
        /// <param name="mongoConnection">MongoDB connection string.</param>
        /// <param name="isDebugMode">If true, connects to debug database.</param>
        public MongoDbContext(string mongoConnection, bool isDebugMode = false)
        {
            Console.WriteLine("MongoDB - Initializing Async...");

            var settings = MongoClientSettings.FromConnectionString(mongoConnection);
            _mongoClient = new MongoClient(settings);

            _database = isDebugMode
                ? _mongoClient.GetDatabase("Dynastio_Debug")
                : _mongoClient.GetDatabase("Dynastio");
        }

        /// <summary>
        /// Initializes MongoDB session asynchronously.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await _mongoClient.StartSessionAsync();
                await DoWorkAsync();
            }
            catch
            {
                // Log: database is not connected
            }
        }

        /// <summary>
        /// Placeholder for initial database tasks after connection.
        /// </summary>
        private async Task DoWorkAsync()
        {
            //var users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("W:\\Halun\\Dynastio.Bot\\database\\newUsersDb.json"));

            //foreach (var user in users)
            //{
            //    if (user.HasLinkedAccount)
            //    {
            //        var account = user.GetDefaultAccount().Clone();
            //        account.AsDefault(true);

            //        user.ClearAccounts();

            //        user.AddAccount(account);
            //    }
            //    else
            //    {
            //        user.ClearAccounts();
            //    }
            //}
            //   await _users.InsertManyAsync(users);


            //File.WriteAllText("newUsersDb.json", JsonSerializer.Serialize(newUsers));

            // Reserved for data migration or background operations
            await Task.CompletedTask;
        }

        #region Guild Operations

        public async Task<Guild> GetGuildAsync(ulong id)
        {
            var result = _guilds.AsQueryable().FirstOrDefault(g => g.Id == id);
            return await Task.FromResult(result);
        }

        public async Task<bool> InsertGuildAsync(Guild guild)
        {
            _guilds.InsertOne(guild);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateGuildAsync(Guild guild)
        {
            _guilds.ReplaceOne(g => g.Id == guild.Id, guild);
            return await Task.FromResult(true);
        }

        public async Task<List<Guild>> GetGuildsAsync(Func<Guild, bool> predicate)
        {
            var result = _guilds.AsQueryable().Where(predicate).ToList();
            return await Task.FromResult(result);
        }

        public async Task<bool> UpdateManyGuildsAsync(List<Guild> guildList)
        {
            var updates = guildList.Select(guildItem =>
            {
                var filter = Builders<Guild>.Filter.Where(g => g.Id == guildItem.Id);
                return new ReplaceOneModel<Guild>(filter, guildItem);
            }).ToList();

            await _guilds.BulkWriteAsync(updates, new BulkWriteOptions { IsOrdered = false });
            return await Task.FromResult(true);
        }

        #endregion

        #region User Operations

        public async Task<User> GetUserAsync(ulong id)
        {
            var result = _users.AsQueryable().FirstOrDefault(u => u.Id == id);
            return await Task.FromResult(result);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(Builders<User>.Filter.Empty).ToListAsync();
        }

        public async Task<bool> InsertUserAsync(User userEntity)
        {
            _users.InsertOne(userEntity);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateUserAsync(User userEntity)
        {
            _users.ReplaceOne(u => u.Id == userEntity.Id, userEntity);
            return await Task.FromResult(true);
        }

        public async Task<User?> GetUserByAccountIdAsync(string accountId)
        {

            var filter = Builders<User>.Filter.ElemMatch(
                u => u.Accounts,
                account => account.Id == accountId
            );

            return await _users
                .Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<User> GetUserByConnectedAccountIdAsync(string gameAccountId)
        {
            return await GetUserByAccountIdAsync(gameAccountId);
        }

        public async Task<User> GetUserByYoutubeChannelIdAsync(string channelId)
        {
            var filter = Builders<User>.Filter.Where(u => u.YouTubeChannel == channelId);
            return await Task.FromResult(_users.Find(filter).FirstOrDefault());
        }

        public async Task<bool> UpdateManyUsersAsync(List<User> userList)
        {
            var updates = userList.Select(userItem =>
            {
                var filter = Builders<User>.Filter.Where(u => u.Id == userItem.Id);
                return new ReplaceOneModel<User>(filter, userItem);
            }).ToList();

            await _users.BulkWriteAsync(updates, new BulkWriteOptions { IsOrdered = false });
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteUserAsync(User userEntity)
        {
            _users.DeleteOne(u => u.Id == userEntity.Id);
            return await Task.FromResult(true);
        }

        #endregion
    }
}
