using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// File-based implementation of IDynastioDatabase for local persistence.
    ///
    /// Interface alignment notes:
    /// - InsertAsync(Guild)      -> InsertGuildAsync(Guild)
    /// - UpdateAsync(Guild)      -> UpdateGuildAsync(Guild)
    /// - InsertAsync(User)       -> InsertUserAsync(User)
    /// - UpdateAsync(User)       -> UpdateUserAsync(User)
    /// - UpdateManyAsync(Users)  -> UpdateManyUsersAsync(List<User>)
    /// - DeleteAsync(User)       -> DeleteUserAsync(User)
    /// - Added: GetAllUsersAsync(), UpdateManyGuildsAsync(List<Guild>)
    ///
    /// Implementation notes:
    /// - Replaced explicit interface implementations using old names with public methods that exactly match IDynastioDatabase.
    /// - Kept SemaphoreSlim for thread-safety and persisted to JSON atomically.
    /// - Avoided expression-bodied members inside try/finally; used explicit return statements to prevent compile errors.
    /// </summary>
    internal sealed class FileDbContext : IDynastioDatabase
    {
        private readonly string _basePath;
        private readonly string _usersFile;
        private readonly string _guildsFile;
        private readonly SemaphoreSlim _lock = new(1, 1);

        // In-memory data
        private List<User> _users = new();
        private List<Guild> _guilds = new();

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public FileDbContext(string basePath)
        {
            _basePath = string.IsNullOrWhiteSpace(basePath)
                ? Path.Combine(AppContext.BaseDirectory, "database")
                : basePath;

            _usersFile = Path.Combine(_basePath, "users.json");
            _guildsFile = Path.Combine(_basePath, "guilds.json");
        }

        // -------------------- Initialization --------------------

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_basePath);
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                _users = await ReadFileAsync<List<User>>(_usersFile).ConfigureAwait(false) ?? new List<User>();
                _guilds = await ReadFileAsync<List<Guild>>(_guildsFile).ConfigureAwait(false) ?? new List<Guild>();
            }
            finally
            {
                _lock.Release();
            }
        }

        // -------------------- Guild Methods (match interface) --------------------

        public async Task<Guild> GetGuildAsync(ulong id)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _guilds.FirstOrDefault(g => g.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Guild>> GetGuildsAsync(Func<Guild, bool> predicate)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _guilds.Where(predicate).ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        // Renamed from InsertAsync(Guild)
        public async Task<bool> InsertGuildAsync(Guild guild)
        {
            if (guild is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_guilds.Any(g => g.Id == guild.Id))
                    return false;

                _guilds.Add(guild);
                await PersistGuildsAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        // Renamed from UpdateAsync(Guild)
        public async Task<bool> UpdateGuildAsync(Guild guild)
        {
            if (guild is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var idx = _guilds.FindIndex(g => g.Id == guild.Id);
                if (idx < 0) return false;

                _guilds[idx] = guild;
                await PersistGuildsAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        // New per interface
        public async Task<bool> UpdateManyGuildsAsync(List<Guild> guildList)
        {
            if (guildList is null || guildList.Count == 0) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var map = guildList.ToDictionary(g => g.Id, g => g);

                // Update existing
                for (int i = 0; i < _guilds.Count; i++)
                {
                    if (map.TryGetValue(_guilds[i].Id, out var updated))
                        _guilds[i] = updated;
                }

                // Insert missing
                foreach (var g in guildList)
                {
                    if (_guilds.All(x => x.Id != g.Id))
                        _guilds.Add(g);
                }

                await PersistGuildsAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        // -------------------- User Methods (match interface) --------------------

        public async Task<User> GetUserAsync(ulong id)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _users.FirstOrDefault(u => u.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        // Implemented: returns a shallow copy to prevent external mutation
        public Task<List<User>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.ToList());
        }

        // Renamed from InsertAsync(User)
        public async Task<bool> InsertUserAsync(User userEntity)
        {
            if (userEntity is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_users.Any(u => u.Id == userEntity.Id))
                    return false;

                _users.Add(userEntity);
                await PersistUsersAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        // Renamed from UpdateAsync(User)
        public async Task<bool> UpdateUserAsync(User userEntity)
        {
            if (userEntity is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var idx = _users.FindIndex(u => u.Id == userEntity.Id);
                if (idx < 0) return false;

                _users[idx] = userEntity;
                await PersistUsersAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<User> GetUserByAccountIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return await Task.FromResult<User>(null).ConfigureAwait(false);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _users.FirstOrDefault(u =>
                {
                    try
                    {
                        return u.GetAccount(accountId) != null;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<User> GetUserByConnectedAccountIdAsync(string connectedAccountId)
        {
            if (string.IsNullOrWhiteSpace(connectedAccountId))
                return await Task.FromResult<User>(null).ConfigureAwait(false);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _users.FirstOrDefault(u => u.HasAccount(connectedAccountId));
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<User> GetUserByYoutubeChannelIdAsync(string youtubeChannelId)
        {
            if (string.IsNullOrWhiteSpace(youtubeChannelId))
                return await Task.FromResult<User>(null).ConfigureAwait(false);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return _users.FirstOrDefault(u =>
                    string.Equals(u.YouTubeChannel, youtubeChannelId, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                _lock.Release();
            }
        }

        // Renamed from UpdateManyAsync(List<User>)
        public async Task<bool> UpdateManyUsersAsync(List<User> userList)
        {
            if (userList is null || userList.Count == 0) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var map = userList.ToDictionary(u => u.Id, u => u);

                // Update existing
                for (int i = 0; i < _users.Count; i++)
                {
                    if (map.TryGetValue(_users[i].Id, out var updated))
                        _users[i] = updated;
                }

                // Insert missing
                foreach (var u in userList)
                {
                    if (_users.All(x => x.Id != u.Id))
                        _users.Add(u);
                }

                await PersistUsersAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        // Renamed from DeleteAsync(User)
        public async Task<bool> DeleteUserAsync(User userEntity)
        {
            if (userEntity is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var removed = _users.RemoveAll(u => u.Id == userEntity.Id) > 0;
                if (removed)
                    await PersistUsersAsync().ConfigureAwait(false);

                return removed;
            }
            finally
            {
                _lock.Release();
            }
        }

        // -------------------- Persistence Helpers --------------------

        private async Task PersistUsersAsync()
        {
            await WriteFileAtomicAsync(_usersFile, _users).ConfigureAwait(false);
        }

        private async Task PersistGuildsAsync()
        {
            await WriteFileAtomicAsync(_guildsFile, _guilds).ConfigureAwait(false);
        }

        private async Task<T> ReadFileAsync<T>(string path)
        {
            if (!File.Exists(path)) return default;
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions).ConfigureAwait(false);
        }

        private async Task WriteFileAtomicAsync<T>(string path, T value)
        {
            var temp = path + ".tmp";
            await using (var stream = File.Create(temp))
            {
                await JsonSerializer.SerializeAsync(stream, value, _jsonOptions).ConfigureAwait(false);
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temp, path);
        }
    }
}
