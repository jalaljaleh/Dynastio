using Dynastio.Bot.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal class EventsHandler : HandlersBase
    {
        private readonly guilds_new_guild _guildsNew;
        private readonly ready_event _ready_event;
        public EventsHandler(IServiceProvider services) : base(services)
        {
            _guildsNew = new guilds_new_guild(services);
            _ready_event = new ready_event(services);
        }
      
    }
}
