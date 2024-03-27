using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    public abstract class HandlersBase
    {
        public readonly IServiceProvider _services;
        public readonly DiscordSocketClient _discord;
        public readonly DynastioBotDatabase _db;
        public readonly AppConfiguration _config;
        protected HandlersBase(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _db = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<AppConfiguration>();
        }
     

       
    }
}
