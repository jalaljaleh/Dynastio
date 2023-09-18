using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Utilities;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class FeaturedVideosService
    {
        private readonly DynastioClient _dynastioClient;
        private readonly RepeaterService _repeaterService;
        private readonly DiscordSocketClient _client;
        private IServiceProvider _services;
        public FeaturedVideosService(IServiceProvider services)
        {
            _services = services;
            _dynastioClient = _services.GetService<DynastioClient>();
            _client = _services.GetService<DiscordSocketClient>();
            _repeaterService = _services.GetRequiredService<RepeaterService>();

            _client.Ready += _client_Ready;
        }

        private async Task _client_Ready()
        {
            _repeaterService.AddAction(RefreshChannelAsync, TimeSpan.FromMinutes(35));
        }
        const ulong _channelId = 1136917780516585472;

        private async Task RefreshChannelAsync()
        {
            var postChannel = _client.Guilds.First().GetTextChannel(_channelId);
            if (postChannel == null) return;
            var expireChannel = _client.Guilds.First().GetTextChannel(1137030131970494524);

            var msgs = await ChannelUtilities.GetChannelMessageAsync(postChannel, 3000);

            List<IMessage> posts = msgs
                .Where(a => a.Source == MessageSource.Bot)
                .ToList();

            foreach (var video in _dynastioClient.FeaturedVideos.OrderByDescending(a => a.ExpireAt))
            {
                var post = posts.FirstOrDefault(a => a.Content.Contains(video.Url));
                if (post is null)
                {
                    await PostVideoAsync(postChannel, video);
                }
                else
                {
                    posts.Remove(post);
                }
            }

            foreach (var x in posts)
            {
                await ExpireVideoAsync(x, expireChannel)
                    .TryAsync();
                
                await Task.Delay(Global.Main.Random.Next(500, 5000));
            };

            await postChannel.DeleteMessagesAsync(posts);
        }

        public async Task PostVideoAsync(ITextChannel channel, FeaturedVideos video)
        {
            var msg = await channel.SendMessageAsync(
                        $"## ✦•··························• Dynast.io •··························•✦\n" +
                        $"\n### Expire {video.ExpireAt.ToDiscordUnixTimestampFormat()}" +
                        "\nUrl: " + video.Url +
                        "\nGroup: " + video.Group +
                        "\nPriority: " + video.Priority);

            await Task.Delay(80);

            await msg.AddReactionAsync(new Emoji("👍"));

            await Task.Delay(Global.Main.Random.Next(150, 1000));
        }
        public async Task ExpireVideoAsync(IMessage msg, ITextChannel channel)
        {
            var content = msg.Content.Replace("Expire", "Expired");
            var msg1 = await channel.SendMessageAsync(
                content +
                "\n### Likes: " + (msg.Reactions?.FirstOrDefault().Value.ReactionCount ?? 0));

            await Task.Delay(80);
           
            await msg1.CrosspostAsync()
                .TryAsync();
        }
    }
}
