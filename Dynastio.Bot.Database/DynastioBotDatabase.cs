using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// High-level database facade for Dynastio.Bot.
    /// Wraps either a MongoDB or file-based backend and adds in-memory caching
    /// for <see cref="Guild"/> and <see cref="User"/> entities to reduce I/O.
    /// </summary>
    public class DynastioBotDatabase
    {
        private IDynastioDatabase _dbContext;
        private readonly ConcurrentBag<User> _users = new();
        private readonly ConcurrentBag<Guild> _guilds = new();

        /// <summary>
        /// Supported storage backends.
        /// </summary>
        public enum DatabaseBackend
        {
            MongoDb,
            FileDb
        }

        /// <summary>
        /// Initializes the underlying <see cref="IDynastioDatabase"/> implementation.
        /// Must be called before any other operations.
        /// </summary>
        /// <param name="connectionString">
        /// MongoDB connection string or base path for file store.
        /// </param>
        /// <param name="backend">Which storage backend to use.</param>
        /// <param name="isDebug">
        /// If true and using MongoDB, connects to the debug database.
        /// Ignored for the file-based backend.
        /// </param>
        public async Task InitializeAsync( string connectionString, DatabaseBackend backend = DatabaseBackend.MongoDb,   bool isDebug = false)
        {
            switch (backend)
            {
                case DatabaseBackend.MongoDb:
                    _dbContext = new MongoDbContext(connectionString, isDebug);
                    break;

                case DatabaseBackend.FileDb:
                    _dbContext = new FileDbContext(connectionString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(backend), "Unsupported database backend");
            }

            await _dbContext.InitializeAsync();
        }

        /// <summary>
        /// Clears the in-memory cache of users and guilds.
        /// </summary>
        public void ClearCache()
        {
            _users.Clear();
            _guilds.Clear();
        }

        // -------------------------------------------------------
        // Guild operations with caching and optional creation
        // -------------------------------------------------------

        /// <summary>
        /// Retrieves a guild by its ID, loading from cache or backend.
        /// Optionally creates and persists a new instance if not found.
        /// </summary>
        /// <param name="id">The Discord guild ID.</param>
        /// <param name="allowCreate">
        /// If true, will call <paramref name="createInstance"/> or new up a <see cref="Guild"/> when missing.
        /// </param>
        /// <param name="createInstance">
        /// Factory to produce a new <see cref="Guild"/>. If null, uses <c>new Guild {{ Id = id }}</c>.
        /// </param>
        /// <param name="onCreate">
        /// Callback invoked on the newly created <see cref="Guild"/> before persisting.
        /// </param>
        /// <returns>The existing or newly created <see cref="Guild"/>.</returns>
        public async Task<Guild> GetGuildAsync(
            ulong id,
            bool allowCreate = true,
            Func<Guild> createInstance = null,
            Action<Guild> onCreate = null)
        {
            // Check in-memory cache first
            var cached = _guilds.FirstOrDefault(g => g.Id == id);
            if (cached != null)
                return cached;

            // Load from backend
            var guild = await _dbContext.GetGuildAsync(id).ConfigureAwait(false);
            if (guild != null)
            {
                _guilds.Add(guild);
                return guild;
            }

            if (!allowCreate)
                return null;

            // Create a new guild instance
            guild = createInstance?.Invoke() ?? new Guild { Id = id };
            onCreate?.Invoke(guild);

            // Persist and cache
            await _dbContext.InsertGuildAsync(guild).ConfigureAwait(false);
            _guilds.Add(guild);
            return guild;
        }

        /// <summary>
        /// Persists updates to an existing guild.
        /// </summary>
        public async Task<bool> UpdateGuildAsync(Guild guild)
        {
            return await _dbContext.UpdateGuildAsync(guild).ConfigureAwait(false);
        }

        /// <summary>
        /// Persists a brand-new guild.
        /// </summary>
        public async Task<bool> InsertGuildAsync(Guild guild)
        {
            return await _dbContext.InsertGuildAsync(guild).ConfigureAwait(false);
        }

        // -------------------------------------------------------
        // User operations with caching and optional creation
        // -------------------------------------------------------

        /// <summary>
        /// Retrieves a user by their Discord ID, loading from cache or backend.
        /// Optionally creates and persists a new instance if not found.
        /// </summary>
        /// <param name="id">The Discord user ID.</param>
        /// <param name="allowCreate">
        /// If true, will call <paramref name="createInstance"/> or new up a <see cref="User"/> when missing.
        /// </param>
        /// <param name="createInstance">
        /// Factory to produce a new <see cref="User"/>. If null, uses <c>new User {{ Id = id }}</c>.
        /// </param>
        /// <param name="onCreate">
        /// Callback invoked on the newly created <see cref="User"/> before persisting.
        /// </param>
        /// <returns>The existing or newly created <see cref="User"/>.</returns>
        public async Task<User> GetUserAsync(
            ulong id,
            bool allowCreate = true,
            Func<User> createInstance = null,
            Action<User> onCreate = null)
        {
            // Check in-memory cache
            var cached = _users.FirstOrDefault(u => u.Id == id);
            if (cached != null)
                return cached;

            // Load from backend
            var user = await _dbContext.GetUserAsync(id).ConfigureAwait(false);
            if (user != null)
            {
                Cache(user);
                return user;
            }

            if (!allowCreate)
                return null;

            // Create new user
            user = createInstance?.Invoke() ?? new User { Id = id };
            onCreate?.Invoke(user);

            // Persist and cache
            await _dbContext.InsertUserAsync(user).ConfigureAwait(false);
            Cache(user);
            return user;
        }

        /// <summary>
        /// Persists all users and returns the full list from the backend.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.GetAllUsersAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Persists updates to an existing user.
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _dbContext.UpdateUserAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a user by YouTube channel ID, using cache or backend.
        /// </summary>
        public async Task<User> GetUserByYoutubeChannelIdAsync(string channelId)
        {
            var cached = _users.FirstOrDefault(u => u.YouTubeChannel == channelId);
            if (cached != null)
                return cached;

            var user = await _dbContext.GetUserByYoutubeChannelIdAsync(channelId).ConfigureAwait(false);
            if (user != null)
                Cache(user);

            return user;
        }

        /// <summary>
        /// Retrieves a user by connected game-account ID, using cache or backend.
        /// </summary>
        public async Task<User> GetUserByConnectedAccountIdAsync(string gameAccountId)
        {
            var cached = _users.FirstOrDefault(u => u.HasAccount(gameAccountId));
            if (cached != null)
                return cached;

            var user = await _dbContext.GetUserByConnectedAccountIdAsync(gameAccountId).ConfigureAwait(false);
            if (user != null)
                Cache(user);

            return user;
        }

        /// <summary>
        /// Retrieves a user by a linked account ID (e.g. Google:123), using cache or backend.
        /// </summary>
        public async Task<User> GetUserByAccountIdAsync(string accountId)
        {
            var cached = _users.FirstOrDefault(u => u.GetAccount(accountId) != null);
            if (cached != null)
                return cached;

            var user = await _dbContext.GetUserByAccountIdAsync(accountId).ConfigureAwait(false);
            if (user != null)
                Cache(user);

            return user;
        }

        // -------------------------------------------------------
        // Local cache helper
        // -------------------------------------------------------

        /// <summary>
        /// Adds the user to the in-memory cache.
        /// </summary>
        private void Cache(User user)
        {
            _users.Add(user);
        }
    }
}
