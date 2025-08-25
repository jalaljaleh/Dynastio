using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.XpRankingSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.EventHandlers
{
    /// <summary>
    /// Handles the DiscordSocketClient.Ready event:
    ///  - Schedules periodic status updates
    ///  - Notifies the application team
    /// </summary>
    public class BotReadyEvent : IDiscordEvent
    {
        private readonly IServiceProvider _services;
        private readonly DynastioBotDatabase _database;
        private readonly ConfigurationService _config;
        private readonly RepeaterService _repeater;
        private readonly DiscordSocketClient _discord;
        private readonly UsersService _usersService;

        public BotReadyEvent(IServiceProvider services)
        {
            _services = services;
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _config = services.GetRequiredService<ConfigurationService>();
            _repeater = services.GetRequiredService<RepeaterService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();

            _usersService = services.GetRequiredService<UsersService>();
        }
        private async Task RunDeveloperDebugActionsAsync()
        {
            //Console.WriteLine( "Developer Debug Actions");

            //var guild = _discord.Guilds.FirstOrDefault(a => a.Id == 480416088312774657);

            //var roles = RoleHelper.GetLatestRoleStartWith(guild, "rank: ");
            //var he = RoleHelper.GetNextHigherHeaderRole(guild, "rank: ");


            //await guild.DownloadUsersAsync();

            //var botGuild = await _database.GetGuildAsync(guild.Id);
            //botGuild.XpSystemSettings.XpRandom = 10;
            //botGuild.XpSystemSettings.XpPerMessage = 60;
            //botGuild.XpSystemSettings.XpBoosters = 10;
            //botGuild.XpSystemSettings.IsGameRewardEnabled = false;
            //botGuild.XpSystemSettings.IsEnabled = true;
            //botGuild.XpSystemSettings.IsRankingRoleAssignmentEnabled = true;
            //botGuild.XpSystemSettings.RankingRoleAssignmentPerfix = "rank: ";

            //botGuild.BadgeBridgeSettings.IsEnabled = true;
            //botGuild.BadgeBridgeSettings.BadgesRoleAssignmentPerfix = "badge: ";

            //Console.WriteLine("Developer Debug Actions      Guild Members and Guild Settings Updated");

            //var guildMembersId = guild.Users
            //    .Where(
            //    a=> a.Roles.Select(a=>a.Name).Any(a=>a.Contains("badge")) || 
            //    a.Roles.Select(a => a.Name).Any(a=>a.Contains("rank")))
            //    .OrderByDescending(a=>a.Roles.Count)
            //    .Select(a => a.Id)
            //    .ToList();

            //var users = await _database.GetAllUsersAsync();
            //var filteredUsers = users.Where(a => guildMembersId.Contains(a.Id)).ToList();

            //foreach (var user in filteredUsers)
            //{
            //    try
            //    {
            //        var member = guild.Users.FirstOrDefault(a => a.Id == user.Id);
            //        var res = await _usersService.SyncUserRolesAsync(member);

            //        Console.WriteLine($"{DateTime.UtcNow}: Developer Debug Actions > Roles Updater : [{filteredUsers.IndexOf(user).ToRegularCounter()}/{filteredUsers.Count}] User {member.Username} --> " + res.ToString());
            //    }
            //    catch
            //    {
            //        Console.WriteLine(  "error");
            //    }

            //    await Task.Delay(100);
            //}

        }
        /// <summary>
        /// Subscribes to the Ready event.
        /// </summary>
        public void Register(DiscordSocketClient client)
        {
            client.Ready += OnReadyAsync;
            //    _logger.LogInformation("ReadyEventHandler registered on DiscordSocketClient.Ready");
        }

        /// <summary>
        /// Unsubscribes from the Ready event.
        /// </summary>
        public void Unregister(DiscordSocketClient client)
        {
            client.Ready -= OnReadyAsync;
            // _logger.LogInformation("ReadyEventHandler unregistered from DiscordSocketClient.Ready");
        }

        /// <summary>
        /// Fired once the client is connected and ready.
        /// </summary>
        private async Task OnReadyAsync()
        {
            try
            {
                // Schedule periodic status updates every 10 minutes
                _repeater.AddAction(UpdateBotStatusAsync, TimeSpan.FromMinutes(10));
                //  _logger.LogInformation("Scheduled periodic status updates");

                // Send a ready notification to the first owner on the app's team
                await NotifyTeamOwnersAsync();
                //  _logger.LogInformation("Sent ready notification to team owners");

                await RunDeveloperDebugActionsAsync();
            }
            catch (Exception ex)
            {
                //  _logger.LogError(ex, "Error in ReadyEventHandler.OnReadyAsync");
            }
        }

        /// <summary>
        /// Sends a "ready" message to the first team owner of the application.
        /// </summary>
        private async Task NotifyTeamOwnersAsync()
        {
            var appInfo = await _services
                .GetRequiredService<DiscordSocketClient>()
                .GetApplicationInfoAsync()
                .ConfigureAwait(false);

            var owner = appInfo.Team?.TeamMembers?.FirstOrDefault()?.User;
            if (owner != null)
                await owner.SendMessageAsync("Bot is now ready!").ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the bot's status and activity.
        /// </summary>
        private async Task UpdateBotStatusAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();
            var totalGuilds = client.Guilds.Count;
            var totalMembers = client.Guilds.Sum(g => g.MemberCount);
            var activityText = $"{totalGuilds} servers · {totalMembers:N0} members";

            await client.SetStatusAsync(UserStatus.Idle).ConfigureAwait(false);
            await client.SetGameAsync(
                    activityText,
                    "https://www.youtube.com/watch?v=v74AQTvjtSg",
                    ActivityType.Streaming)
                .ConfigureAwait(false);

            //_logger.LogDebug("Updated status: {Activity}", activityText);

        }


    }
}