using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Dynastio.Data;
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
using Dynastio.Bot.Global;

namespace Dynastio.Bot.Handlers
{
    internal class MemberUpdatesHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;


        public MemberUpdatesHandler(IServiceProvider services)
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

        readonly Dictionary<string, ulong> roleHeaders = new Dictionary<string, ulong>()
        {
            { "ver:", 1121835049344569416},
            { "ach:", 1113119762277482647},
            { "mod:", 1113119662100709406},
            { "badge:", 1113080837303455794},
            { "rank:", 1113082402781282334},
            { "bot:", 1149645412781801553},
        };
        async Task RolesChangedAsync(SocketGuildUser user)
        {
            List<ulong> rolesToAdd = new List<ulong>();
            List<ulong> rolesToRemove = new List<ulong>();

            foreach (var header in roleHeaders)
            {
                CheckUserRoles(header.Key, header.Value, user, out ulong add, out ulong remove);

                if (add != 0)
                    rolesToAdd.Add(add);

                if (remove != 0)
                    rolesToRemove.Add(remove);
            }

            if (rolesToAdd.Any())
                await user.AddRolesAsync(rolesToAdd);

            if (rolesToRemove.Any())
                await user.RemoveRolesAsync(rolesToAdd);
        }
        void CheckUserRoles(string startWith, ulong categoryId, SocketGuildUser user, out ulong add, out ulong remove)
        {
            add = 0;
            remove = 0;

            var hasRole = user.Roles.Any(a => a.Name.StartsWith(startWith));
            var hasHeader = user.Roles.Any(a => a.Id == categoryId);

            if (hasRole && !hasHeader)
            {
                add = categoryId;
            }

            if (!hasRole && hasHeader)
            {
                remove = categoryId;
            }
        }
    }
}
