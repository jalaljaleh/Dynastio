using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Managers;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal class EventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly GuildService _guildService;
        private readonly UserService _usersService;
        private readonly RankService _rankService;
        private readonly GraphicService _graphicService;
        private readonly DynastioClient _dynastioClient;

        public EventHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _configuration = _services.GetRequiredService<Configuration>();
            _guildService = _services.GetService<GuildService>();
            _rankService = _services.GetService<RankService>();
            _usersService = _services.GetService<UserService>();
            _graphicService = _services.GetService<GraphicService>();
            _dynastioClient = _services.GetService<DynastioClient>();

            _client.Ready += _client_Ready;
            _client.UserJoined += _client_UserJoined;


        }
        private const ulong _onlinePlayersChannelId = 1109014316922978354;
        private const ulong _onlineServersChannelId = 1109019161587355738;

        public async Task UpdateInformationChannelsAsync()
        {
            var guild = _client.GetGuild(GuildService._officialGuildId);

            await guild.GetTextChannel(_onlineServersChannelId)
                .ModifyAsync(x => x.Name = $"Online Servers: {_dynastioClient.OnlineServers.Where(a=>a.IsPrivate == false).Count()}");
           
            await guild.GetTextChannel(_onlinePlayersChannelId)
                .ModifyAsync(x => x.Name = $"Online Players: {_dynastioClient.OnlinePlayers.Count}");
        }

        public static ulong _memberChannelId = 1109020050163240990;
        private async Task _client_UserJoined(SocketGuildUser user)
        {
            try
            {
                await user.SendMessageAsync(
                    text: "https://www.youtube.com/watch?v=x1tRXvcFJvs" + "\n" + "https://dynast.io/",
                    components: new ComponentBuilder()
                            .WithButton("Play Dynast.io", null, ButtonStyle.Link, null, "https://dynast.io/")
                            .WithButton("Youtube Channel", null, ButtonStyle.Link, null, "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1")
                .Build());
            }
            catch { }

            await DiscordStream.SendFileAsync(
                channel: user.Guild.GetTextChannel(_memberChannelId),
                img: await _graphicService.GetWelcomeImage(user),
                user.Id + ".jpg",
                user.Id.ToUserMention(),
                embed: new EmbedBuilder()
                {
                    Description = $"A wild {user.Id.ToUserMention()} appears !",
                    ImageUrl = $"attachment://{user.Id}.jpg"
                }.Build());
        }

        private async Task _client_Ready()
        {
            var guilds = _client.Guilds.ToList();

            await _client.SetGameAsync($"Watching {guilds.Sum(a => a.MemberCount)} members !", "", ActivityType.Playing);
            await _client.SetStatusAsync(UserStatus.Online);

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await UpdateInformationChannelsAsync();
                    await Task.Delay(TimeSpan.FromSeconds(120));
                }
            });
        }
    }
}
