using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Dynastio.Bot.EventHandlers
{
    /// <summary>
    /// When the bot joins a guild, ensures we stay under the configured guild limit.
    /// If we've exceeded the threshold, leaves the smallest guilds first.
    /// </summary>
    public class GuildJoinedEvent : IDiscordEvent
    {
        private const int DefaultMaxGuilds = 95;

        private readonly DiscordSocketClient _client;
        private readonly ConfigurationService _config;
        private readonly UsersService _usersService;
        private readonly int _maxGuilds;

        public GuildJoinedEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigurationService>();
            _usersService = services.GetRequiredService<UsersService>();

            _maxGuilds = _config.GuildLimit > 0 ? _config.GuildLimit : DefaultMaxGuilds;
        }

        /// <summary>
        /// Subscribe to the JoinedGuild event.
        /// </summary>
        public void Register(DiscordSocketClient client)
        {
            client.JoinedGuild += OnJoinedGuildAsync;
            // _logger.LogInformation("GuildJoinedHandler registered.");
        }

        /// <summary>
        /// Unsubscribe from the JoinedGuild event.
        /// </summary>
        public void Unregister(DiscordSocketClient client)
        {
            client.JoinedGuild -= OnJoinedGuildAsync;
            //_logger.LogInformation("GuildJoinedHandler unregistered.");
        }

        /// <summary>
        /// Fired whenever the bot joins a new guild.
        /// If the total guild count exceeds <see cref="_maxGuilds"/>,
        /// leaves the smallest guilds until we're back under the limit.
        /// </summary>
        private async Task OnJoinedGuildAsync(SocketGuild newGuild)
        {

            try
            {
                var total = _client.Guilds.Count;
                if (total >= _maxGuilds)
                {

                    var excess = total - _maxGuilds;
                    //       _logger.LogWarning("Joined guild {GuildName} ({GuildId}). Guild count {Total} exceeds limit {Limit} by {Excess}.",newGuild.Name, newGuild.Id, total, _maxGuilds, excess);

                    var toLeave = _client.Guilds
                        .OrderBy(g => g.MemberCount)
                        .Take(excess);

                    foreach (var g in toLeave)
                    {
                        await g.LeaveAsync();
                        //             _logger.LogInformation( "Left guild {GuildName} ({GuildId}) with {MemberCount} members.",g.Name, g.Id, g.MemberCount);
                    }
                }
            }
            catch (Exception ex)
            {
                //           _logger.LogError( ex,   "Error while handling JoinedGuild for {GuildId}.",     newGuild.Id);
            }


            /// This is a unsafe code and should not be like this !
            try
            {
                Program.UnsafeCode = true;
                const ulong role = 1279386778180255794;
                const ulong guildId = 480416088312774657;

              var buser = await _usersService.GetOrCreateUserAsync(newGuild.OwnerId);
                if (buser.IsBanned)
                {
                    await newGuild.LeaveAsync();
                    return;
                }

                var dynastioGuild = _client.Guilds.FirstOrDefault(g => g.Id == guildId);
                foreach (var g in _client.Guilds)
                {
                    var owner = g.Owner?.Id ?? g?.OwnerId ?? 0;
                    if (owner == 0) continue;

                    var ownerUser = dynastioGuild.GetUser(owner);
                    if (ownerUser == null) continue;

                    if (ownerUser.Roles.Any(a => a.Id == role) is false)
                    {
                        await ownerUser.AddRoleAsync(role);
                        Console.WriteLine("add owner roles");
                    }

                    await Task.Delay(500);
                }

                var owners = _client.Guilds.Select(a => a.OwnerId).ToList();
                foreach (var user in dynastioGuild.Roles.First(a => a.Id == role).Members)
                {
                    try
                    {
                        if (owners.Contains(user.Id) is false)
                        {
                            await user.RemoveRoleAsync(role);
                        }
                    }
                    catch { }
                    await Task.Delay(500);
                }
            }
            catch { }
        }
    }
}