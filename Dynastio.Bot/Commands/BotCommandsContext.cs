using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands
{
    public class BotSocketCommandContext : SocketCommandContext
    {
        public readonly IServiceProvider _services;
        public readonly DynastioBotDatabase Database;
        public readonly ClientService ClientService;

        private User _user;
        private Guild _guild;
        public BotSocketCommandContext(IServiceProvider services, DiscordSocketClient client, SocketUserMessage msg, User user = null, Guild guild = null) : base(client, msg)
        {
            Database = services.GetRequiredService<DynastioBotDatabase>();
            ClientService = services.GetRequiredService<ClientService>();

            _services = services;

            _user = user;
            _guild = guild;
        }

        /// <summary>
        /// Gets the bot's internal user model for the current interaction user.
        /// </summary>
        public User BotUser => _user ??= Database.GetUserAsync(User.Id, true).Result;

        /// <summary>
        /// Gets the bot's internal guild model for the current interaction guild.
        /// </summary>
        public Guild BotGuild => _guild ??= Database.GetGuildAsync(Guild.Id).Result;
    }
}
