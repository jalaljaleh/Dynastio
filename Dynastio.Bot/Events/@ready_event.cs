using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Entities;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;

namespace Dynastio.Bot.Events
{
    public class ready_event 
    {
        private readonly RepeaterService _repeaterService;
        private readonly DynastioBotDatabase _db;
        private readonly AppConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public ready_event(IServiceProvider services) 
        {
            _services = services;
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<AppConfiguration>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _repeaterService = services.GetRequiredService<RepeaterService>();
            
            _discord.Ready += _discord_Ready;
        }

        private async Task _discord_Ready()
        {

            _repeaterService.AddAction(SetBotStatus, TimeSpan.FromMinutes(10));
            await SendMessageToTeamOwners().TryAsync();
            await SyncGuildPartnerRoles().TryAsync();
        }
        public async Task SyncGuildPartnerRoles()
        {
            var subscribedGuilds = await _db.GetSubscriptioGuildsAsync();
            subscribedGuilds = subscribedGuilds
                                .Where(a => a.PartnersRoleId != 0)
                                .ToList();

            var partners = _discord.Guilds.Select(a => a.OwnerId).ToList();

            foreach (var subscribedGuild in subscribedGuilds)
            {
                if (subscribedGuild.PartnersRoleId == 0)
                    continue;

                var dGuild = _discord.GetGuild(subscribedGuild.Id);
                if (dGuild is null) continue; // if guild not available (left or bot kicked)

                // get partner role
                var guildRole = dGuild.Roles.FirstOrDefault(a => a.Id == subscribedGuild.PartnersRoleId);
                if (guildRole is null)
                {
                    subscribedGuild.PartnersRoleId = 0;
                    continue;
                }

                try
                {
                    //remove extra users
                    foreach (var member in guildRole.Members)
                    {

                        if (partners.Contains(member.Id))
                            partners.Remove(member.Id);

                        else await member.RemoveRoleAsync(guildRole);
                    }

                    // add role to user
                    foreach (var partner in partners)
                    {
                        var member = dGuild.GetUser(partner);
                        if (member != null)
                        {
                            if (member.Roles.Any(a => a.Id == subscribedGuild.PartnersRoleId))
                                continue;

                            await member.AddRoleAsync(subscribedGuild.PartnersRoleId);
                            await Task.Delay(1000);
                        }
                    }
                }
                catch
                {
                    subscribedGuild.PartnersRoleId = 0;
                    await _db.UpdateAsync(subscribedGuild);
                }
            }
        }

        public async Task SendMessageToTeamOwners()
        {
            var _application = await _discord.GetApplicationInfoAsync();
            await _application.Team.TeamMembers.FirstOrDefault().User.SendMessageAsync("ready ..");
        }
        public async Task SetBotStatus()
        {
            await _discord.SetStatusAsync(UserStatus.Idle);

            await _discord.SetGameAsync(_discord.Guilds.Count + " Servers " + _discord.Guilds.Select(a => a.MemberCount).Sum().Metric() + " Members", "https://www.youtube.com/watch?v=v74AQTvjtSg", ActivityType.Streaming);
        }

    }
}
