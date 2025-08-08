using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot
{
    /// <summary>
    /// Centralizes registration and cleanup of all Discord event handlers.
    /// </summary>
    internal sealed class EventsHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IEnumerable<IDiscordEvent> _eventHandlers;


        /// <summary>
        /// Pulls in every IDiscordEvent implementation from DI.
        /// </summary>
        public EventsHandler(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _eventHandlers = services.GetServices<IDiscordEvent>();

        }

        /// <summary>
        /// Hooks up all event handlers to the Discord client.
        /// </summary>
        public Task InitializeAsync()
        {
            foreach (var handler in _eventHandlers)
            {
                handler.Register(_client);
                //  _logger.LogInformation("Registered event handler {HandlerName}", handler.GetType().Name);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Unhooks every handler when disposing.
        /// </summary>

        public void Dispose()
        {
            foreach (var handler in _eventHandlers)
            {
                handler.Unregister(_client);
                // _logger.LogDebug("Unregistered event handler {HandlerName}", handler.GetType().Name);
            }
        }
    }
}