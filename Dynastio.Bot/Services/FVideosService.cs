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
    public class FVideosService
    {
        private readonly DynastioClient _dynastioClient;
        private readonly RepeaterService _repeaterService;
        private readonly DiscordSocketClient _client;
        private readonly GuildService _guildService;
        private IServiceProvider _services;
        public FVideosService(IServiceProvider services)
        {
            _services = services;
            _dynastioClient = _services.GetService<DynastioClient>();
            _client = _services.GetService<DiscordSocketClient>();
            _repeaterService = _services.GetRequiredService<RepeaterService>();
            _guildService = _services.GetRequiredService<GuildService>();

            _client.Ready += _client_Ready;
        }

        private async Task _client_Ready()
        {
            _repeaterService.AddAction(RefreshChannelAsync, TimeSpan.FromMinutes(120),TimeSpan.FromMinutes(20));
        }

        private async Task RefreshChannelAsync()
        {
            var postChannel = _guildService.GetTextChannel(SavedChannels.GuildChannelType.FeaturedVideos);
            if (postChannel == null) return;

            var expireChannel = _guildService.GetTextChannel(SavedChannels.GuildChannelType.FeaturedVideosExpired);
            if (expireChannel == null) return;

            var msgs = await ChannelUtilities.GetChannelMessageAsync(postChannel, 3000);

            List<IMessage> posts = msgs
                .Where(a => a.Source == MessageSource.Bot)
                .ToList();

            int i = 0;
            foreach (var video in _dynastioClient.FeaturedVideos.OrderByDescending(a => a.ExpireAt))
            {
                try
                {
                    var post = posts.FirstOrDefault(a => a.Content.Contains(video.Url));
                    if (post is null && i < 5)
                    {
                        i++;
                        await PostVideoAsync(postChannel, video);
                    }
                    else
                        posts.Remove(post);
                }
                catch
                {
                }
            }

            foreach (var x in posts)
            {
                try
                {
                    await ExpireVideoAsync(x, expireChannel)
                        .TryAsync();

                    await Task.Delay(Global.Main.Random.Next(1000, 5000));
                }
                catch
                {
                }
            };

            var result = await postChannel.DeleteMessagesAsync(posts).TryAsync();
            if (result is false)
            {
                posts.ForEach(async a =>
                {
                    await a.DeleteAsync();
                    await Task.Delay(800);
                });
            }
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

            await msg.AddReactionAsync(new Emoji("👍"))
                .TryAsync();

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
