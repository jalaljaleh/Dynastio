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
        private readonly guilds_new_event _guildsNew;
        public EventsHandler(IServiceProvider services) : base(services)
        {
            _guildsNew = new guilds_new_event(services);
        }
      
    }
}
