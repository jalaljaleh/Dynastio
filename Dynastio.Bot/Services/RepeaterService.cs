using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{

    public class RepeaterService
    {
        private readonly Dictionary<Func<Task>, Timer> actionTimers;
        private IServiceProvider _services;
        public RepeaterService(IServiceProvider services)
        {
            _services = services;
            actionTimers = new();
        }
        public static void RepeatAction(Action action, int repeatCount, TimeSpan delay)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                action();
                Task.Delay(delay).GetAwaiter().GetResult();
            }
        }
        public static async Task RepeatActionAsync(Func<Task> action, int repeatCount, TimeSpan delay)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                await action.Invoke();
                await Task.Delay(delay);
            }
        }
        public void AddAction(Func<Task> action, TimeSpan interval, TimeSpan dueTime = default)
        {
            var timer = new Timer(_ =>
            {
                action.Invoke();
            }, null, dueTime, interval);

            actionTimers[action] = timer;
        }

        public void RemoveAction(Func<Task> action)
        {
            if (actionTimers.TryGetValue(action, out var timer))
            {
                timer.Dispose();
                actionTimers.Remove(action);
            }
        }

        public void RemoveAllActions()
        {
            foreach (var kvp in actionTimers)
            {
                kvp.Value.Dispose();
            }

            actionTimers.Clear();
        }
    }
}
