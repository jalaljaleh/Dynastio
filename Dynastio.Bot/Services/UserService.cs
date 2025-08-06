using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class UserService : ServicesBase
    {
        private readonly BadgesBridgeService badgesService;
        private readonly XpRankingSystemService rankingService;
        public UserService(IServiceProvider services) : base(services)
        {
            badgesService = services.GetRequiredService<BadgesBridgeService>();
            rankingService = services.GetRequiredService<XpRankingSystemService>();
        }
        public async Task<bool> SyncUserRolesAsync(IGuildUser user)
        {
            var BotGuild = await _db.GetGuildAsync(user.Guild.Id);
            var BotUser = await _db.GetUserAsync(user.Id);
            return await SyncUserRolesAsync(BotGuild, BotUser, user);
        }
        public async Task<bool> SyncUserRolesAsync(Guild BotGuild, User BotUser, IGuildUser user)
        {
            var badgesResult = await badgesService.SynchronizeUserRolesAsync(BotGuild, user, BotUser).TryAsync();
            
            var rankingResult = await XpRankingSystem.RoleSyncService.SynchronizeUserRolesAsync(BotGuild, user, BotUser.GetServerProfile(BotGuild.Id).Level).TryAsync();

            return true;
        }
       
        public async Task<bool> UpdateUserAsync(User user)
        {
            user.LastUpdateTime = DateTime.UtcNow;
            return await _db.UpdateAsync(user);
        }
    }
}
