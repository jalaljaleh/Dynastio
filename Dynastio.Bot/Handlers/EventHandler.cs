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
using System.Text.RegularExpressions;
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
        private readonly RepeaterService _repeaterService;

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
            _repeaterService = _services.GetRequiredService<RepeaterService>();

            _client.Ready += _client_Ready;
            _client.UserJoined += _client_UserJoined;


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
            // dev channel
            await _client.Guilds.First().GetTextChannel(1109911020341837825).SendMessageAsync("Ready !");

            _repeaterService.AddFunction(updateStatus(), TimeSpan.FromMinutes(10));
        }
        async Task updateStatus()
        {
            int serversCount = _dynastioClient.OnlineServers.Where(a => a.IsPrivate == false).Count();
            int playerscount = _dynastioClient.OnlinePlayers.Count;

            await _client.SetGameAsync($"Online players {playerscount}, Online servers {serversCount} !", "", ActivityType.Playing);
        }


    }
}
