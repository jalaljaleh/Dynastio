using Dynastio.Bot.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    public class EventsHandler : HandlersBase
    {
        public readonly guilds_join_guild _guildsNew;
        public readonly guilds_left_guild _guildsremove;
        public readonly ready_event _ready_event;
        public EventsHandler(IServiceProvider services) : base(services)
        {
            _guildsNew = new guilds_join_guild(services);
            _guildsremove = new guilds_left_guild(services);
            _ready_event = new ready_event(services);
        }
      
    }
}
