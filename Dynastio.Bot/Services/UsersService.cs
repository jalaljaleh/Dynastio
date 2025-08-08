using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services.XpRankingSystem;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    /// <summary>
    /// Provides user-related operations such as role synchronization and profile updates.
    /// </summary>
    public class UsersService
    {
        private readonly DynastioApi _dynast;
        private readonly DynastioBotDatabase _db;
        private readonly BadgesRoleSyncService _badgesService;
        private readonly XpRankingSystemService _rankingService;

        public UsersService(IServiceProvider services)
        {
            _dynast = services.GetRequiredService<DynastioApi>();
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _badgesService = services.GetRequiredService<BadgesRoleSyncService>();
            _rankingService = services.GetRequiredService<XpRankingSystemService>();
        }
        public async Task<User> GetUserAsync(ulong Id, bool AllowCreate = true)
        {
            var user = await _db.GetUserAsync(Id, true, () => UserFactory.CreateDefault(Id), null);
            return user;
        }
        /// <summary>
        /// Synchronizes roles for a Discord user based on their badges and XP level.
        /// </summary>
        /// <param name="user">The Discord guild user.</param>
        /// <returns><c>true</c> if synchronization was attempted.</returns>
        public async Task<bool> SyncUserRolesAsync(IGuildUser user)
        {
            var botGuild = await _db.GetGuildAsync(user.Guild.Id);
            var botUser = await _db.GetUserAsync(user.Id);

            return await SyncUserRolesAsync(botGuild, botUser, user);
        }

        /// <summary>
        /// Synchronizes roles for a user using provided bot data.
        /// </summary>
        public async Task<bool> SyncUserRolesAsync(Guild botGuild, User botUser, IGuildUser user)
        {
            var badgeSync = await _badgesService.SynchronizeUserRolesAsync(botGuild, user, botUser)
                .TryAsync();

            var level = botUser.GetServerProfile(botGuild.Id)?.Level ?? 0;

            var xpSync = await XpRankingSystemServiceHelper.AssignmentUserRolesAsync(botGuild, user, level)
                .TryAsync();

            return badgeSync.isSuccessful && xpSync.isSuccessful;
        }

        /// <summary>
        /// Updates the user record with the current timestamp.
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            user.LastUpdateTime = DateTime.UtcNow;
            return await _db.UpdateAsync(user);
        }
    }
}