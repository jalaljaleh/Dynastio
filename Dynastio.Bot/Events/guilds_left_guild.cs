using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Entities;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot.Events
{
    public class guilds_left_guild
    {
        private readonly DynastioBotDatabase _db;
        private readonly AppConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        public guilds_left_guild(IServiceProvider services)
        {
            _services = services;
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<AppConfiguration>();
            _discord = services.GetRequiredService<DiscordSocketClient>();

            _discord.LeftGuild += _discord_Left_Guild;
        }

        private async Task _discord_Left_Guild(Discord.WebSocket.SocketGuild newGuild)
        {
            await RemoveSubscriptionGuildAdminRoleAsync(newGuild).TryAsync();

        }
        public async Task RemoveSubscriptionGuildAdminRoleAsync(SocketGuild newGuild)
        {
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
                            await owner.RemoveRoleAsync(subscribedGuild.PartnersRoleId);
                    }
                }
                catch
                {
                    subscribedGuild.PartnersRoleId = 0;
                    await _db.UpdateAsync(subscribedGuild);
                }
            }
        }
    }
}
