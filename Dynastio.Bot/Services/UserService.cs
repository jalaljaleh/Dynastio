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
        private readonly BadgesService badgesService;
        private readonly RankingService rankingService;
        public UserService(IServiceProvider services) : base(services)
        {
            badgesService = services.GetService<BadgesService>();
            rankingService = services.GetService<RankingService>();
        }
        public async Task<bool> SyncUserRolesAsync(IGuildUser user)
        {
            var BotGuild = await _db.GetGuildAsync(user.Guild.Id);
            var BotUser = await _db.GetUserAsync(user.Id);
            return await SyncUserRolesAsync(BotGuild, BotUser, user);
        }
        public async Task<bool> SyncUserRolesAsync(Guild BotGuild, User BotUser, IGuildUser user)
        {
            var rankingResult = await rankingService.SynchronizeUserRolesAsync(BotGuild, user, BotUser.GetRankingProfile(BotGuild.Id).Level)
                               .TryAsync();
            var badgesResult = await badgesService.SynchronizeUserRolesAsync(BotGuild, user, BotUser)
                               .TryAsync();
            return rankingResult.isSuccesful && badgesResult.isSuccesful;
        }
       
        public async Task<bool> UpdateUserAsync(User user)
        {
            user.LastUpdateTime = DateTime.UtcNow;
            return await _db.UpdateAsync(user);
        }
    }
}
