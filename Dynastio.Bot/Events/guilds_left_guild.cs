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
    public class guilds_left_guild : HandlersBase
    {
        public guilds_left_guild(IServiceProvider services) : base(services)
        {
            _discord.LeftGuild += _discord_Left_Guild;
        }

        private async Task _discord_Left_Guild(Discord.WebSocket.SocketGuild newGuild)
        {
            await RemoveSubscriptionGuildAdminRoleAsync(newGuild).TryAsync();

        }
        public async Task RemoveSubscriptionGuildAdminRoleAsync(SocketGuild newGuild)
        {
            var subscriptionGuilds = await _db.GetSubscriptioGuildsAsync();
            foreach (var subscribedGuild in subscriptionGuilds.Where(a => _discord.GetGuild(a.Id) != null))
            {
                if (subscribedGuild.TryGetRole(Database.RoleType.SubscriptionGuildAdmin, out ulong adminRole) is false)
                    continue;

                try
                {
                    var discordSubscribedGuild = _discord.GetGuild(subscribedGuild.Id);
                    var owner = discordSubscribedGuild.GetUser(newGuild.OwnerId);

                    if (owner != null)
                    {
                        await owner.RemoveRoleAsync(adminRole);
                    }
                }
                catch
                {
                    subscribedGuild.TryRemoveRole(Database.RoleType.SubscriptionGuildAdmin);
                    await _db.UpdateAsync(subscribedGuild);
                }
            }
        }
    }
}
