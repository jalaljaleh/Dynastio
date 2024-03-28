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
        public Task AddSubscriptionGuildAdminRoleAsync(SocketGuild newGuild)
        {
            _ = Task.Run(async () =>
             {

                 await Task.Delay(5000);

                 if (_discord.Guilds.Any(a => a.Id == newGuild.Id) is false) return;

                 var subscriptionGuilds = await _db.GetSubscriptioGuildsAsync();
                 subscriptionGuilds = subscriptionGuilds
                                     .Where(a => a.PartnersRoleId != 0)
                                     .ToList();

                 foreach (var subscribedGuild in subscriptionGuilds)
                 {
                     if (subscribedGuild.PartnersRoleId == 0)
                         continue;

                     var dGuild = _discord.GetGuild(subscribedGuild.Id);
                     if (dGuild is null) continue; // if guild not available (left or bot kicked)

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

            return Task.CompletedTask;
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
