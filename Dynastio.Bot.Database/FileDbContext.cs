using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    // File-based implementation of IDynastioDatabase
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

        async Task IDynastioDatabase.InitializeAsync()
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

        // ---------- Guilds ----------

        async Task<Guild> IDynastioDatabase.GetGuildAsync(ulong id)
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

        async Task<List<Guild>> IDynastioDatabase.GetGuildsAsync(Func<Guild, bool> predicate)
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

        async Task<bool> IDynastioDatabase.InsertAsync(Guild guild)
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

        async Task<bool> IDynastioDatabase.UpdateAsync(Guild guild)
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

        // ---------- Users ----------

        // Note: The interface oddly marks this as 'internal'. Implement explicitly to avoid visibility issues.
        async Task<User> IDynastioDatabase.GetUserAsync(ulong id)
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

        async Task<bool> IDynastioDatabase.InsertAsync(User user)
        {
            if (user is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_users.Any(u => u.Id == user.Id))
                    return false;

                _users.Add(user);
                await PersistUsersAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        async Task<bool> IDynastioDatabase.UpdateAsync(User user)
        {
            if (user is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var idx = _users.FindIndex(u => u.Id == user.Id);
                if (idx < 0) return false;

                _users[idx] = user;
                await PersistUsersAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        async Task<User> IDynastioDatabase.GetUserByAccountIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return await Task.FromResult<User>(null);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Assuming a method User.GetAccount(string) exists per your usage elsewhere
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

        async Task<User> IDynastioDatabase.GetUserByConnectedAccountIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return await Task.FromResult<User>(null);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Matches your calling code: x => x.gameAccountId == accountId
                return _users.FirstOrDefault(u => string.Equals(u.gameAccountId, accountId, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                _lock.Release();
            }
        }

        async Task<User> IDynastioDatabase.GetUserByYoutubeChannelIdAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
                return await Task.FromResult<User>(null);

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Matches your calling code: x => x.youtube_channel == channelurl
                return _users.FirstOrDefault(u => string.Equals(u.youtube_channel, channelId, StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                _lock.Release();
            }
        }

        async Task<bool> IDynastioDatabase.UpdateManyAsync(List<User> users)
        {
            if (users is null || users.Count == 0) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var map = users.ToDictionary(u => u.Id, u => u);
                for (int i = 0; i < _users.Count; i++)
                {
                    if (map.TryGetValue(_users[i].Id, out var updated))
                        _users[i] = updated;
                }

                // Insert missing
                foreach (var u in users)
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

        async Task<bool> IDynastioDatabase.DeleteAsync(User user)
        {
            if (user is null) return false;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var removed = _users.RemoveAll(u => u.Id == user.Id) > 0;
                if (removed)
                    await PersistUsersAsync().ConfigureAwait(false);

                return removed;
            }
            finally
            {
                _lock.Release();
            }
        }

        // ---------- Persistence Helpers ----------

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
