using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;

namespace Dynastio.Bot.Events
{
    public class guilds_join_guild : HandlersBase
    {
        public guilds_join_guild(IServiceProvider services) : base(services)
        {
            _discord.JoinedGuild += _discord_JoinedGuild;
        }

        private async Task _discord_JoinedGuild(SocketGuild newGuild)
        {
            await AddSubscriptionGuildAdminRoleAsync(newGuild).TryAsync();
            await LeaveExtraGuildsAsync().TryAsync();
        }
        public async Task AddSubscriptionGuildAdminRoleAsync(SocketGuild newGuild)
        {
            _ = Task.Run(async () =>
             {

                 await Task.Delay(5000);

                 var subscriptionGuilds = await _db.GetSubscriptioGuildsAsync();
                 foreach (var subscribedGuild in subscriptionGuilds.Where(a => _discord.GetGuild(a.Id) != null))
                 {
                     if (subscribedGuild.PartnersRoleId == 0)
                         continue;

                     try
                     {
                         var discordSubscribedGuild = _discord.GetGuild(subscribedGuild.Id);
                         var owner = discordSubscribedGuild.GetUser(newGuild.OwnerId);

                         if (owner != null)
                         {
                             if (owner.Roles.Select(a => a.Id).Contains(subscribedGuild.PartnersRoleId))
                                 continue;

                             await owner.AddRoleAsync(subscribedGuild.PartnersRoleId);
                         }
                     }
                     catch
                     {
                         subscribedGuild.PartnersRoleId = 0;
                         await _db.UpdateAsync(subscribedGuild);
                     }
                 }
             });
        }
        public async Task LeaveExtraGuildsAsync()
        {
            // remove extra guilds for unverified bots..
            if (_discord.Guilds.Count > 75)
            {
                foreach (var removeG in _discord.Guilds.OrderBy(a => a.MemberCount))
                {
                    var removebGuild = await _db.GetGuildAsync(removeG.Id);
                    if (removebGuild.HasSubscription() == false)
                    {
                        await removeG.LeaveAsync();
                        break;
                    }
                }

            }
        }
    }
}
