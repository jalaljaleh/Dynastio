
namespace Dynastio.Bot.Handlers
{
    using Discord;
    using Discord.Rest;
    using Discord.WebSocket;
    using Dynastio.Data;
    using Microsoft.Extensions.DependencyInjection;
    internal class InviteHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GuildService _guildService;
        private readonly DynastioData _usersService;
        private readonly RankService _rankService;
        private readonly GraphicService _graphicService;

        public InviteHandler(IServiceProvider services)
        {
            _services = services;
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _guildService = _services.GetService<GuildService>();
            _rankService = _services.GetService<RankService>();
            _usersService = _services.GetService<DynastioData>();
            _graphicService = _services.GetService<GraphicService>();

            _discord.UserJoined += _client_UserJoined;
        }

        private async Task _client_UserJoined(SocketGuildUser joinedUser)
        {
            await DiscordStream.SendFileAsync(
                channel: joinedUser.Guild.GetTextChannel(1109020050163240990),
                img: await _graphicService.GetWelcomeImage(joinedUser),
                joinedUser.Id + ".jpg",
                joinedUser.Id.ToUserMention(),
            embed: new EmbedBuilder()
            {
                Description = $"A wild {joinedUser.Id.ToUserMention()} appears !",
                ImageUrl = $"attachment://{joinedUser.Id}.jpg"
            }.Build());
        }
    }
}
