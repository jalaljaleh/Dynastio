
namespace Dynastio.Bot.Handlers
{
    using Discord.Rest;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    internal class InviteHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GuildService _guildService;
        private readonly UserService _usersService;
        private readonly RankService _rankService;

        public InviteHandler(IServiceProvider services)
        {
            _services = services;
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _guildService = _services.GetService<GuildService>();
            _rankService = _services.GetService<RankService>();
            _usersService = _services.GetService<UserService>();

            _discord.Ready += _client_Ready;
            _discord.UserJoined += _client_UserJoined;
            _discord.InviteCreated += _discord_InviteCreated;
        }


        private IReadOnlyCollection<RestInviteMetadata> _cachedInviteMetadata;
        private async Task _client_UserJoined(SocketGuildUser joinedUser)
        {
            if (joinedUser.IsBot) return;

            var newInvitesMetadata = await joinedUser.Guild.GetInvitesAsync();

            var currentInvite = _cachedInviteMetadata.FirstOrDefault(
                                                a => newInvitesMetadata
                                                    .Where(
                                                     x =>
                                                     x.Id == a.Id &&
                                                     x.Uses - 1 == a.Uses)
                                                    .Any());
            if (currentInvite is null) return;

            _cachedInviteMetadata = newInvitesMetadata;

            var newBotUser = await _usersService.GetUserAsync(joinedUser.Id, false);
            if (newBotUser is not null) return;

            newBotUser = await _usersService.GetUserAsync(joinedUser.Id, true);

            var botUserInviter = await _usersService.GetUserAsync(currentInvite.Inviter.Id, false);
            if (botUserInviter is null) return;

            await _rankService.AddXpAsync(botUserInviter, _discord.CurrentUser.Id, 500, $"{joinedUser.Id.ToUserMention()} joined to the server by your currentInvite link.");
        }

        private async Task _discord_InviteCreated(SocketInvite arg)
        {
            _cachedInviteMetadata = await arg.Guild.GetInvitesAsync();
        }
        private async Task _client_Ready()
        {
            _cachedInviteMetadata = await _discord.GetGuild(GuildService._officialGuildId).GetInvitesAsync();
        }
    }
}
