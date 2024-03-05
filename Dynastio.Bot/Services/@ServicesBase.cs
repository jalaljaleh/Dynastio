using Discord.WebSocket;
using Dynastio.Bot.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public abstract class ServicesBase
    {
        public readonly IServiceProvider _services;
        public readonly DiscordSocketClient _discord;
        public readonly DynastioBotDatabase _db;

        public ServicesBase(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _db = services.GetRequiredService<DynastioBotDatabase>();

        }
    }
}
