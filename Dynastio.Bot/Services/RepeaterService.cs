using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    /// <summary>
    /// Provides functionality to repeat asynchronous actions at intervals or a fixed number of times.
    /// </summary>
    public class RepeaterService
    {
        private readonly Dictionary<Func<Task>, Timer> _actionTimers;
        private readonly IServiceProvider _services;

        public RepeaterService(IServiceProvider services)
        {
            _services = services;
            _actionTimers = new();
        }

        /// <summary>
        /// Repeats a synchronous action a specified number of times with a delay between each execution.
        /// </summary>
        public static void RepeatAction(Action action, int repeatCount, TimeSpan delay)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                action();
                Task.Delay(delay).GetAwaiter().GetResult(); // Blocking delay
            }
        }

        /// <summary>
        /// Repeats an asynchronous action a specified number of times with a delay between each execution.
        /// </summary>
        public static async Task RepeatActionAsync(Func<Task> action, int repeatCount, TimeSpan delay)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                await action();
                await Task.Delay(delay);
            }
        }

        /// <summary>
        /// Adds an asynchronous action to be executed repeatedly at a specified interval.
        /// </summary>
        /// <param name="action">The asynchronous action to repeat.</param>
        /// <param name="interval">The interval between executions.</param>
        /// <param name="dueTime">Optional initial delay before the first execution.</param>
        public void AddAction(Func<Task> action, TimeSpan interval, TimeSpan dueTime = default)
        {
            var timer = new Timer(async _ =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    // Optional: log or handle the exception
                    Console.WriteLine($"RepeaterService error: {ex.Message}");
                }
            }, null, dueTime, interval);

            _actionTimers[action] = timer;
        }

        /// <summary>
        /// Removes a previously added repeating action.
        /// </summary>
        public void RemoveAction(Func<Task> action)
        {
            if (_actionTimers.TryGetValue(action, out var timer))
            {
                timer.Dispose();
                _actionTimers.Remove(action);
            }
        }

        /// <summary>
        /// Removes and disposes all repeating actions.
        /// </summary>
        public void RemoveAllActions()
        {
            foreach (var timer in _actionTimers.Values)
            {
                timer.Dispose();
            }

            _actionTimers.Clear();
        }
    }
}