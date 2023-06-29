using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class RepeaterService
    {
        private Dictionary<Task, Timer> functionTimers;
        private IServiceProvider _services;
        public RepeaterService(IServiceProvider services)
        {
            functionTimers = new Dictionary<Task, Timer>();
            _services = services;

        }

        public void AddFunction(Task function, TimeSpan interval, TimeSpan dueTime = default)
        {
            var timer = new Timer(async _ =>
            {
                await function;
            }, null, dueTime , interval);

            functionTimers[function] = timer;
        }
        public void RemoveFunction(Task function)
        {
            functionTimers[function].Dispose();
        }

        public void RemoveAll()
        {
            foreach (var kvp in functionTimers)
            {
                Timer timer = kvp.Value;
                timer.Dispose();
            }

            functionTimers.Clear();
        }
    }
}
