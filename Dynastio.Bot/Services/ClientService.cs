using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class ClientService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private Cacheable<RestApplication> _application;
        public ClientService(IServiceProvider service)
        {
            _services = service;
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _application = new Cacheable<RestApplication>(TimeSpan.FromMinutes(10), async () => await _client.GetApplicationInfoAsync());
        }
        public bool HasTeamRole(ulong userId,params string[] TeamRoles)
        {
            var application = _application.Value;

            var idFound = false;

            foreach (var member in application.Team.TeamMembers)
            {
                if (member.User.Id == userId)
                {
                    if (TeamRoles.Length == 0 || TeamRoles.Any(role => member.Permissions.Contains(role)))
                    {
                        idFound = true;
                    }

                    break;
                }
            }
            return idFound;
        }
    }
}
