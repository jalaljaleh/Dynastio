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
                await RolesChangedAsync(newUser);
            }
        }

        async Task RolesChangedAsync(SocketGuildUser user)
        {
            await CheckCategorie("ach:", 1113119762277482647, user);
            await CheckCategorie("mod:", 1113119662100709406, user);
            await CheckCategorie("badge:", 1113080837303455794, user);
            await CheckCategorie("rank:", 1113082402781282334, user);
        }
        async Task CheckCategorie(string startWith, ulong categoryId, SocketGuildUser user)
        {
            var hasModroles = user.Roles.Any(a => a.Name.StartsWith(startWith));
            var hasModHeader = user.Roles.Any(a => a.Id == categoryId);

            if (hasModroles && !hasModHeader)
                await user.AddRoleAsync(categoryId);
            if (!hasModroles && hasModHeader)
                await user.RemoveRoleAsync(categoryId);
        }
    }
}
