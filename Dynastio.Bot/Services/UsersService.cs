using System;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services.XpRankingSystem;
using Microsoft.Extensions.Logging;

namespace Dynastio.Bot.Services
{
    /// <summary>
    /// Manages Discord user data and synchronizes guild roles
    /// based on badge achievements and XP levels.
    /// </summary>
    public class UsersService
    {
        private readonly DynastioBotDatabase _db;
        private readonly BadgesRoleSyncService _badgesSync;

        /// <summary>
        /// Constructor with required dependencies injected.
        /// </summary>
        public UsersService(DynastioBotDatabase db, BadgesRoleSyncService badgesSync)
        {
            _db = db;
            _badgesSync = badgesSync;
        }

        /// <summary>
        /// Fetches an existing user or creates a new default record.
        /// </summary>
        public async Task<User> GetOrCreateUserAsync(ulong discordUserId, bool alloCreate = true)
        {
            return await _db.GetUserAsync(discordUserId, allowCreate: alloCreate);
        }
        public async Task<User> GetUserByAccountIdAsync(string accountId) { var user = await _db.GetUserByAccountIdAsync(accountId); return user; }
        /// <summary>
        /// Synchronizes badge and XP roles for a guild user.
        /// Runs both syncs in parallel to minimize latency.
        /// </summary>
        public async Task<bool> SyncUserRolesAsync(IGuildUser guildUser)
        {
            // 1. Load bot-side guild and user records in parallel
            var guildTask = _db.GetGuildAsync(guildUser.Guild.Id);
            var userTask = _db.GetUserAsync(guildUser.Id);

            await Task.WhenAll(guildTask, userTask);

            var botGuild = guildTask.Result;
            var botUser = userTask.Result;

            // 2. Determine current XP level (defaults to 0)
            int level = botUser.TryGetGuildProfile(botGuild.Id)?.Level ?? 0;

            // 3. Start both sync operations without waiting
            var badgeSyncTask = _badgesSync
                .SynchronizeUserRolesAsync(botGuild, guildUser, botUser)
                .TryAsync();

            var xpSyncTask = botGuild.XpSystemSettings.IsRankingRoleAssignmentEnabled
                ? XpRankingSystemServiceHelper
                    .AssignmentUserRolesAsync(botGuild, guildUser, level)
                    .TryAsync()
                : Task.FromResult<(bool isSuccessful, List<IRole> result)>((true, new List<IRole>()));

            await Task.WhenAll(badgeSyncTask, xpSyncTask);

            var (badgeOk, badgeErr) = await badgeSyncTask;
            var (xpOk, xpErr) = await xpSyncTask;

            if (!badgeOk || !xpOk)
            {
              Console.WriteLine(
                    "Role sync failed for User={UserId} · BadgesOk={BadgeOk} ({BadgeErr}) · XpOk={XpOk} ({XpErr})",
                    guildUser.Id, badgeOk, badgeErr, xpOk, xpErr
                );
            }

            return badgeOk && xpOk;
        }

        /// <summary>
        /// Updates only the LastUpdateTime on a user record.
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            user.Touch();
            return await _db.UpdateUserAsync(user);
        }
    }
}
