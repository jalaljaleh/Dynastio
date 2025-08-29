using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Contract for Dynastio database operations on Guilds and Users.
    /// </summary>
    internal interface IDynastioDatabase
    {
        /// <summary>
        /// Initializes the database context and starts a session if applicable.
        /// </summary>
        Task InitializeAsync();

        #region Guild Methods

        Task<Guild> GetGuildAsync(ulong id);
        Task<List<Guild>> GetGuildsAsync(Func<Guild, bool> predicate);
        Task<bool> InsertGuildAsync(Guild guild);
        Task<bool> UpdateGuildAsync(Guild guild);
        Task<bool> UpdateManyGuildsAsync(List<Guild> guildList);

        #endregion

        #region User Methods

        Task<User> GetUserAsync(ulong id);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> InsertUserAsync(User userEntity);
        Task<bool> UpdateUserAsync(User userEntity);
        Task<User> GetUserByAccountIdAsync(string accountId);
        Task<User> GetUserByConnectedAccountIdAsync(string connectedAccountId);
        Task<User> GetUserByYoutubeChannelIdAsync(string youtubeChannelId);
        Task<bool> UpdateManyUsersAsync(List<User> userList);
        Task<bool> DeleteUserAsync(User userEntity);

        #endregion
    }
}
