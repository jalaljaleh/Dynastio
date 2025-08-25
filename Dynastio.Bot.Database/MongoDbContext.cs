using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Net;
using MongoDB;
using MongoDB.Bson;
using Amazon.Auth.AccessControlPolicy;
using System.Dynamic;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;


namespace Dynastio.Bot.Database
{
    internal class MongoDbContext : IDynastioDatabase
    {
        private MongoClient _db { get; set; }
        private IMongoDatabase _dynastio;
        private IMongoCollection<User> _users => _dynastio.GetCollection<User>("Users");
        private IMongoCollection<Guild> _guilds => _dynastio.GetCollection<Guild>("Guilds");

        public event OnDatabaseMessageLogs OnMessagesLog;
        public delegate void OnDatabaseMessageLogs(string serviceName, string message, ConsoleColor color = default);
        public MongoDbContext(string mongoConnection, bool isDebugMode = false)
        {
            Console.WriteLine("Mongodb" + " Initialize Async..");

            var settings = MongoClientSettings.FromConnectionString(mongoConnection);
            // settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            _db = new MongoClient(settings);

            if (isDebugMode)
                _dynastio = _db.GetDatabase("Dynastio_Debug");
            else
                _dynastio = _db.GetDatabase("Dynastio");

            //    OnMessagesLog.Invoke("Mongodb", "Initialized");
        }
        public async Task InitializeAsync()
        {
            try
            {
                //       OnMessagesLog.Invoke("Mongodb", "Start Session Async ..");
                await _db.StartSessionAsync();

                
                await DoWorkAsync();
               
            }
            catch
            {
                //  OnMessagesLog.Invoke("Mongodb", "db is not connected.", ConsoleColor.Red);
            }
        }
        public async Task DoWorkAsync()
        {


            //var se = JsonSerializer.Serialize(allGuilds);
            //var guilds = JsonSerializer.Deserialize<List<Guild>>(se);
            //await UpdateManyAsync(guilds);



            // await update();
            //var allUsers = await _users.Find(Builders<User>.Filter.Empty).ToListAsync();
            //var se = JsonSerializer.Serialize(allUsers);
            //var users = JsonSerializer.Deserialize<List<User>>(se);
            //await UpdateManyAsync(users);
            //  OnMessagesLog.Invoke("Mongodb", "Session Started.");


        }

        public async Task<Guild> GetGuildAsync(ulong Id)
        {
            var result = _guilds.AsQueryable()
                  .Where(a => a.Id == Id)
                  .FirstOrDefault();
            return await Task.FromResult(result);
        }
        public async Task<bool> InsertAsync(Guild guild)
        {
            _guilds.InsertOne(guild);
            return await Task.FromResult(true);
        }
        public async Task<bool> UpdateAsync(Guild guild)
        {
            _guilds.ReplaceOne(a => a.Id == guild.Id, guild);
            return await Task.FromResult(true);
        }
        public async Task<List<Guild>> GetGuildsAsync(Func<Guild, bool> predicate)
        {
            var result = _guilds.AsQueryable()
                  .Where(predicate)
                  .ToList();
            return await Task.FromResult(result);
        }
        public async Task<bool> UpdateManyAsync(List<Guild> guild)
        {
            var updates = new List<WriteModel<Guild>>();
            foreach (var _advertise in guild)
            {
                var filter = Builders<Guild>.Filter.Where(u => u.Id == _advertise.Id);
                updates.Add(new ReplaceOneModel<Guild>(filter, _advertise));
            }
            await _guilds.BulkWriteAsync(updates, new BulkWriteOptions() { IsOrdered = false });
            return await Task.FromResult(true);
        }



        public async Task<User> GetUserAsync(ulong Id)
        {
            var result = _users.AsQueryable()
                .Where(a => a.Id == Id)
                .FirstOrDefault();

            return await Task.FromResult(result);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            var allUsers = await _users.Find(Builders<User>.Filter.Empty).ToListAsync();
            return allUsers;
        }
        public async Task<bool> InsertAsync(User Buser)
        {
            _users.InsertOne(Buser);
            return await Task.FromResult(true);
        }
        public async Task<bool> UpdateAsync(User Buser)
        {
            _users.ReplaceOne(a => a.Id == Buser.Id, Buser);
            return await Task.FromResult(true);
        }
        public async Task<User> GetUserByAccountIdAsync(string Id)
        {
            var filter = Builders<User>.Filter
                .ElemMatch(o => o.Accounts, Builders<UserGameAccount>.Filter.Where(a => a.Id == Id));

            var result = _users.Find(filter).FirstOrDefault();
            return await Task.FromResult(result);
        }
        public async Task<User> GetUserByConnectedAccountIdAsync(string Id)
        {
            var filter = Builders<User>.Filter.Where(a => a.gameAccountId == Id);
            var result = _users.Find(filter).FirstOrDefault();
            return await Task.FromResult(result);
        }

        public async Task<User> GetUserByYoutubeChannelIdAsync(string Id)
        {
            var filter = Builders<User>.Filter.Where(a => a.youtube_channel == Id);
            var result = _users.Find(filter).FirstOrDefault();
            return await Task.FromResult(result);
        }

        //public async Task<List<User>> GetActivityScoreLeaderboardAsync(int count = 15)
        //{
        //    var filter = Builders<User>.Filter.Empty;

        //    var sort = Builders<User>.Sort
        //        .Descending(a => a.activiy_level)
        //        .Descending(a => a.activiy_score);

        //    var result = await _users.FindAsync(filter, new FindOptions<User, User>()
        //    {
        //        Sort = sort,
        //        Limit = count,
        //    });

        //    return result.ToList();
        //}
        public async Task<bool> UpdateManyAsync(List<User> users)
        {
            var updates = new List<WriteModel<User>>();
            foreach (var user in users)
            {
                var filter = Builders<User>.Filter.Where(u => u.Id == user.Id);
                updates.Add(new ReplaceOneModel<User>(filter, user));
            }
            await _users.BulkWriteAsync(updates, new BulkWriteOptions() { IsOrdered = false });
            return await Task.FromResult(true);
        }
        public async Task<bool> DeleteAsync(User user)
        {
            _users.DeleteOne(a => a.Id == user.Id);
            return await Task.FromResult(true);
        }

    }
}