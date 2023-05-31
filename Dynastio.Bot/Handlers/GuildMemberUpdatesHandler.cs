using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Managers;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal class GuildMemberUpdatesHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;


        public GuildMemberUpdatesHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _client.GuildMemberUpdated += _client_GuildMemberUpdated;
        }

        private async Task _client_GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> _oldUser, SocketGuildUser newUser)
        {
            if (!_oldUser.HasValue) return;
            var oldUser = _oldUser.Value;

            if (newUser.Roles.Count != oldUser.Roles.Count)
            {
                //var changedRoles = newUser.Roles.Except(oldUser.Roles);
                //var addedRoles = changedRoles.Where(a => newUser.Roles.Contains(a));
                //var removedRoles = changedRoles.Where(a => !newUser.Roles.Contains(a));
                await RolesChangedAsync(newUser);
            }
        }
        private const ulong _achievmentHeader = 1113119762277482647;
        private const ulong _moderatorHeader = 1113119662100709406;
        async Task RolesChangedAsync(SocketGuildUser user)
        {
            var hasAchroles = user.Roles.Any(a => a.Name.StartsWith("ach:"));
            var hasAchHeader = user.Roles.Any(a => a.Id == _achievmentHeader);

            if (hasAchroles && !hasAchHeader)
                await user.AddRoleAsync(_achievmentHeader);
            if (!hasAchroles && hasAchHeader)
                await user.RemoveRoleAsync(_achievmentHeader);


            var hasModroles = user.Roles.Any(a => a.Name.StartsWith("mod:"));
            var hasModHeader = user.Roles.Any(a => a.Id == _moderatorHeader);

            if (hasModroles && !hasModHeader)
                await user.AddRoleAsync(_moderatorHeader);
            if (!hasModroles && hasModHeader)
                await user.RemoveRoleAsync(_moderatorHeader);
        }
    }
}
