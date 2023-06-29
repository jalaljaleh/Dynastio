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
using System.Security.Claims;
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
        private readonly IDynastioBotDatabase _database;

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
            _database = _services.GetRequiredService<IDynastioBotDatabase>();

            _client.Ready += _client_Ready;
            _client.UserJoined += _client_UserJoined;

        }

        private async Task _client_UserJoined(SocketGuildUser user)
        {
            await user.SendMessageAsync(
                text: "https://www.youtube.com/watch?v=x1tRXvcFJvs" + "\n" + "https://dynast.io/",
                components: new ComponentBuilder()
                .WithButton("Play Dynast.io", null, ButtonStyle.Link, null, "https://dynast.io/")
                .WithButton("Youtube Channel", null, ButtonStyle.Link, null, "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1")
                .Build())
                .TryAsync();

            await DiscordStream.SendFileAsync(
                channel: user.Guild.GetTextChannel(_guildService.GetChannelId(GuildService.GuildChannelType.MemberChannel)),
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
            if (Global.Main.IsDebug())
                return;


            _repeaterService
                .AddFunction(EidMubarakEvent(), TimeSpan.FromHours(1));

            _repeaterService
                .AddFunction(updateStatus(), TimeSpan.FromMinutes(10));

            await _client.Guilds.First()
                .GetTextChannel(1109911020341837825)
                .SendMessageAsync("Ready !")
                .TryAsync();
        }
        async Task status()
        {
            var channel = _client.Guilds.First()
                .GetTextChannel(1124036365613539408);

            if (channel == null) return;

            var msgs = await channel.GetMessagesAsync().FlattenAsync().TryAsync();
            foreach (var message in msgs.result)
            {
                if (message.Author.IsBot is false)
                    continue;

                await message.DeleteAsync()
                    .TryAsync();
            }

            await channel.SendMessageAsync(
             embed: new EmbedBuilder()
             {

             }.Build())
             .TryAsync();
        }
        async Task updateStatus()
        {
            int serversCount = _dynastioClient.OnlineServers
                .Where(a => a.IsPrivate == false)
                .Count();

            int playerscount = _dynastioClient.OnlinePlayers.Count;

            await _client.SetGameAsync($"{playerscount} players, {serversCount} Servers!", "", ActivityType.Watching);
        }

        async Task EidMubarakEvent()
        {
            var code = await _database.GetRedeemCodeAsync(RedeemCode.RedeemType.Coin_100);
            if (code is null)
                return;

            var result = await _client.Guilds.First()
                .GetTextChannel(1108998382996946964)
                .SendMessageAsync(
                embed:
                ($"Here is a redeem code for ` Eid al-Adha `\n" +
                $"```{code.Code}```\nA code will be sent every 1 hour until 24 hours.").ToEmbed("Eid al-Adha", "https://cdn.discordapp.com/attachments/1098332386674085988/1123659712165056562/happy-eid-ul-adha-6.png", color: Discord.Color.Orange))
                .TryAsync();

            if (result.isSuccesful)
                await _database.DeleteAsync(code);

        }
    }
}
