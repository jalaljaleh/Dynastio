using Dynastio.Bot.Global;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Net;
using MongoDB;
using MongoDB.Bson;

namespace Dynastio.Bot.Data
{
    public class MongoDbContext : IDynastioBotDatabase
    {
        private IMongoDatabase _dynastio;
        private IMongoCollection<User> _users => _dynastio.GetCollection<User>("Users");
        private IMongoCollection<Guild> _guilds => _dynastio.GetCollection<Guild>("Guilds");
        private IMongoCollection<RedeemCode> _redeemCodes => _dynastio.GetCollection<RedeemCode>("RedeemCodes");

        private MongoClient _db { get; set; }
        public MongoDbContext(string mongoConnection)
        {
            Main.Log("Mongodb", "Initialize Async..");

            var settings = MongoClientSettings.FromConnectionString(mongoConnection);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            _db = new MongoClient(settings);

            _dynastio = _db.GetDatabase(Main.IsDebug() ? "Dynastio_Debug" : "Dynastio");

            Main.Log("Mongodb", "Initialized");
        }
        public async Task InitializeAsync()
        {
            try
            {
                Main.Log("Mongodb", "Start Session Async ..");

                await _db.StartSessionAsync();

                Main.Log("Mongodb", "Session Started.");
            }
            catch
            {
                Main.Log("Mongodb", "db is not connected.", ConsoleColor.Red);
            }
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




        public async Task<User> GetUserAsync(ulong Id)
        {
            var result = _users.AsQueryable()
                .Where(a => a.Id == Id)
                .FirstOrDefault();

            return await Task.FromResult(result);
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
                .ElemMatch(o => o.Accounts, Builders<UserAccount>.Filter.Where(a => a.Id == Id));

            var result = _users.Find(filter).FirstOrDefault();
            return await Task.FromResult(result);
        }
        public async Task<List<User>> GetHonorLeaderboardAsync(int count = 10)
        {
            var filter = Builders<User>.Filter.Empty;
            var sort = Builders<User>.Sort.Descending(a => a.Honor);
            var result = await _users.FindAsync(filter, new FindOptions<User, User>()
            {
                Sort = sort,
                Limit = count,
            });
            return result.ToList();
        }
        public async Task<List<User>> GetActivityScoreLeaderboardAsync(int count = 15)
        {
            var filter = Builders<User>.Filter.Empty;
            var sort = Builders<User>.Sort.Descending(a => a.activiy_score);
            var result = await _users.FindAsync(filter, new FindOptions<User, User>()
            {
                Sort = sort,
                Limit = count,
            });
            return result.ToList();
        }
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

        public async Task<RedeemCode> GetRedeemCodeAsync(RedeemCode.RedeemType type)
        {
            var result = this._redeemCodes.AsQueryable()
                       .Where(a => a.Type == type)
                       .FirstOrDefault();
            return await Task.FromResult(result);
        }
        public async Task<List<RedeemCode>> GetRedeemCodesAsync()
        {
            var result = this._redeemCodes.AsQueryable()
                        .ToList();
            return await Task.FromResult(result);
        }
        public async Task<bool> InsertAsync(RedeemCode redeemCode)
        {
            _redeemCodes.InsertOne(redeemCode);
            return await Task.FromResult(true);
        }
        public async Task<bool> InsertManyAsync(List<RedeemCode> redeemCodes)
        {
            _redeemCodes.InsertMany(redeemCodes);
            return await Task.FromResult(true);
        }
        public async Task<bool> DeleteAsync(RedeemCode redeemCodes)
        {
            _redeemCodes.DeleteOne(a => a.Id == redeemCodes.Id);
            return await Task.FromResult(true);
        }
    }
}