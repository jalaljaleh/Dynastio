using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Handlers;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;

namespace Dynastio.Bot.Events
{
    public class ready_event : HandlersBase
    {
        private readonly RepeaterService _repeaterService;
        public ready_event(IServiceProvider services) : base(services)
        {
            _discord.Ready += _discord_Ready;
            _repeaterService = services.GetRequiredService<RepeaterService>();
        }

        private async Task _discord_Ready()
        {

            _repeaterService.AddAction(SetBotStatus, TimeSpan.FromMinutes(10));
            await SendMessageToTeamOwners().TryAsync();
            await SyncSub().TryAsync();
        }
        public async Task SyncSub()
        {
            var owners = _discord.Guilds.Select(a => a.OwnerId);
            var subscriptionGuilds = await _db.GetSubscriptioGuildsAsync();
            foreach (var subscribedGuild in subscriptionGuilds.Where(a => _discord.GetGuild(a.Id) != null))
            {
                if (subscribedGuild.PartnersRoleId == 0)
                    continue;

                foreach (var newGuild in _discord.Guilds)
                {
                    try
                    {
                        var discordSubscribedGuild = _discord.GetGuild(subscribedGuild.Id);

                        var members = discordSubscribedGuild.Roles.FirstOrDefault(a => a.Id == subscribedGuild.PartnersRoleId).Members;
                        foreach(var member in members)
                        {
                            if (owners.Contains(member.Id))
                            {

                            }
                            else
                            {
                               await member.RemoveRoleAsync(subscribedGuild.PartnersRoleId);
                            }
                        }

                        var owner = discordSubscribedGuild.GetUser(newGuild.OwnerId);
                        if (owner != null)
                        {
                            await owner.AddRoleAsync(subscribedGuild.PartnersRoleId);
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
        public async Task SendMessageToTeamOwners()
        {
            var _application = await _discord.GetApplicationInfoAsync();
            await _application.Team.TeamMembers.FirstOrDefault().User.SendMessageAsync("ready ..");
        }
        public async Task SetBotStatus()
        {
            await _discord.SetStatusAsync(UserStatus.Idle);

            await _discord.SetGameAsync(_discord.Guilds.Count + " servers " + _discord.Guilds.Select(a => a.MemberCount).Sum().Metric() + " Members", "https://www.youtube.com/watch?v=v74AQTvjtSg", ActivityType.Streaming);
        }

    }
}
