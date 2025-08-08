using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dynastio.Bot.EventHandlers
{
    /// <summary>
    /// Handles the UserJoined event: synchronizes badges & XP roles when a new member joins.
    /// </summary>
    public class UserJoinedEvent : IDiscordEvent
    {
        private readonly DiscordSocketClient _client;
        private readonly UsersService _usersService;

        public UserJoinedEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _usersService = services.GetRequiredService<UsersService>();
        }

        /// <summary>
        /// Subscribes to the UserJoined event.
        /// </summary>
        public void Register(DiscordSocketClient client)
        {
            client.UserJoined += OnUserJoinedAsync;
           // _logger.LogInformation("UserJoinedHandler registered.");
        }

        /// <summary>
        /// Unsubscribes from the UserJoined event.
        /// </summary>
        public void Unregister(DiscordSocketClient client)
        {
            client.UserJoined -= OnUserJoinedAsync;
           // _logger.LogInformation("UserJoinedHandler unregistered.");
        }

        /// <summary>
        /// Invoked whenever a user joins a guild: triggers role synchronization.
        /// </summary>
        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            try
            {
              //  _logger.LogDebug("Synchronizing roles for new user {UserId} in guild {GuildId}.", user.Id, user.Guild.Id);

                bool success = await _usersService.SyncUserRolesAsync(user);
                if (!success)
                {
                 //   _logger.LogWarning("Role synchronization failed for user {UserId}.", user.Id);
                }
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Error while synchronizing roles for user {UserId} in guild {GuildId}.",user.Id, user.Guild.Id);
            }
        }
    }
}