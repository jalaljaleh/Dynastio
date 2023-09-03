using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Services;
using Dynastio.Bot.Interactions.AutoCompeletes;
using Newtonsoft.Json;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube.v3;
using System.Threading.Channels;
using System.Threading;

namespace Dynastio.Bot.Interactions.Modules
{
    [EnabledInDm(false)]
    public class promotevideoModule : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public YoutubeService _youtubeService { get; set; }
        public IDynastioBotDatabase _database { get; set; }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireRole(480954902005415937)]
        [SlashCommand("disconnect-youtube-channel", "disconnect your youtube channel !")]
        public async Task connectchannel(IUser target)
        {
            await DeferAsync();
            var targetUser = await _userService.GetUserAsync(target.Id, false);
            if (targetUser != null)
            {
                targetUser.youtube_channel = null;
                await _userService.UpdateAsync(targetUser);
            }
            await FollowupAsync(embed: "Operator was succesful".ToEmbed("channel disconnected from the user profile."));
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireRole(480954902005415937)]
        [SlashCommand("promote-video-list", " promote list requests!")]
        public async Task promotelist()
        {
            await DeferAsync();

            var promo = await _database.GetYoutuberVideosAsync();

            var content = promo.ToStringTable(new[] { "#", "User", "Url" },
                a => promo.IndexOf(a),
                a => $"<@{a.user}>",
                a => "https://www.youtube.com/channel/" + a.videoId
               )
                + $"\n`{promo.Count} removed from database.`";

            await FollowupAsync(userMention, embed: content.ToEmbed("Requested Promo Videos"));

            foreach (var item in promo)
            {
                await _database.DeleteAsync(item);
            }
        }

        [SlashCommand("connect-youtube-channel", "connect your youtube channel !")]
        public async Task connectchannel(string channelId)
        {
            await DeferAsync();

            var channelValidation = await _youtubeService.IsChannelExistAsync(channelId);
            if (channelValidation is false)
            {
                await FollowupAsync(embed:
                                  ($"## The channel not found.\n" +
                                  $"- The channel not found, make sure your channel id is correct.\n" +
                                  $"after doing the steps, we will infrom you the result !")
                                  .ToEmbed("Channel not found", Color.Red));
                return;
            }

            if (string.IsNullOrEmpty(BotUser.youtube_channel) is false)
            {
                await FollowupAsync(embed:
                                  ($"## you have connected a channel already\n" +
                                   $"you added a channel alrady, remove the old channel first.")
                                   .ToEmbed("Access Denied"));
                return;
            }

            var checkUsers = await _database.GetUserByYoutubeChannelIdAsync(channelId);
            if (checkUsers is not null)
            {
                await FollowupAsync(embed:
                                  ($"## Channel added by someone else already\n" +
                                   $"<@{checkUsers}> added this channel alrady, if its your channel but someone else added it, infrom us by creating a ticket.")
                                   .ToEmbed("Access Denied"));
                return;
            }

            await Context.Guild.GetTextChannel(1147964955547668600)
                .SendMessageAsync(

                embed: new EmbedBuilder()
                {
                    Title = "Youtube Confirmation Request",
                    Description = $"{Context.User.Mention} sent a request to confirm that the below channel belongs to him.\n" +
                    $"**Channel Url**: https://www.youtube.com/channel/{channelId}",
                    ThumbnailUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Channel Id",
                            Value = channelId,
                            IsInline = true
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "User",
                            Value = Context.User.Mention,
                            IsInline = true
                        }
                    }
                }.Build(),

                components: new ComponentBuilder()
            .WithButton("Deny", $"btn.connectyoutubechannel:deny:{Context.BotUser.Id}:{channelId}", ButtonStyle.Danger)
            .WithButton("Confirm", $"btn.connectyoutubechannel:allow:{Context.BotUser.Id}:{channelId}", ButtonStyle.Success)

            .Build());

            await FollowupAsync(embed:
                           ($"## Request Sent Succesfuly !\n" +
                           $"- Your channel sent to developers to check, we will inform you the result!")
                           .ToEmbed("Operator was succesful", Color.Green));
        }


        [ComponentInteraction("btn.connectyoutubechannel:*:*:*")]
        public async Task btn_connectyoutubechannel(string action, ulong userId, string channel)
        {
            await DeferAsync();

            var user = await Context.Client.GetUserAsync(userId);

            if (action is "deny")
            {
                await user.SendMessageAsync("Your request for connection your youtube channel denied by developers.")
                    .TryAsync();

                await deleteRequest();
                return;
            }

            var checkUsers = await _database.GetUserByYoutubeChannelIdAsync(channel);
            if (checkUsers is not null)
            {
                await FollowupAsync(embed:
                                  ($"## Channel added by someone else already\n" +
                                   $"<@{checkUsers}> added this channel alrady, if its your channel but someone else added it, infrom us by creating a ticket.")
                                   .ToEmbed("Access Denied"));

                await user.SendMessageAsync("Your request for connect your youtube channel denied by developers because your channel added by someone else.")
                   .TryAsync();

                await deleteRequest();
                return;
            }

            var targetUser = await _userService.GetUserAsync(userId);
            if (string.IsNullOrEmpty(targetUser.youtube_channel) is false)
            {
                await FollowupAsync(embed:
                                  ($"## User connected a channel already\n" +
                                   $"<@{targetUser}> added a channel alrady, remove the old channel first.")
                                   .ToEmbed("Access Denied"));

                await user.SendMessageAsync("Your request for connect your youtube channel denied by developers because you have a channel already.")
                   .TryAsync();

                await deleteRequest();
                return;
            }


            targetUser.youtube_channel = channel;

            await _userService.UpdateAsync(targetUser);

            await user.SendMessageAsync("Your request for connect your youtube channel accepted by developers.")
                 .TryAsync();

            await deleteRequest();

            async Task deleteRequest()
            {
                await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
            }
        }

        [SlashCommand("promote-video", "promote your dynastio video !")]
        public async Task promote(string videoId)
        {
            await DeferAsync();

            if (string.IsNullOrEmpty(BotUser.youtube_channel))
            {
                await FollowupAsync(embed:
                    ($"## You have to connect your channel to your bot account first.\n" +
                    $"- connect your youtube channel to your discord account first from **Discord Connections**.`\n" +
                    $"- then use this command to coonect your channel to the bot `/connect-youtube-channel`\n" +
                    $"- wait for developers to confirm your request.\n" +
                    $"after doing the steps, we will infrom you the result !")
                    .ToEmbed("Channel not found", Color.Orange));
                return;
            }


            var yvideo = await _database.GetYotuberVideoAsync(videoId);
            if (yvideo != null)
            {
                await FollowupAsync(embed:
                   ($"## Video uploaded by someone else already\n" +
                    $"<@{yvideo.user}> requested for this video already, if its your video but someone else uploaded it, infrom us by creating a ticket.")
                    .ToEmbed("Access Denied", Color.Red));
                return;
            }



            //var channelVideos = await _youtubeService.GetAllChannelVideos(Context.BotUser.youtube_channel);
            //if(channelVideos.Any(a=>a.Id.VideoId == url))
            //{

            //}

            var videos = await _youtubeService.GetVideoAsync(videoId);
            if (videos is null || videos.Items.Any(a => a.Id == videoId) is false)
            {
                await FollowupAsync(embed:
                           ($"## Video not found\n" +
                           $"- your video not found, make sure you are sending the video id only !")
                           .ToEmbed("Not Found", Color.Orange));
                return;
            }

            var video = videos.Items.FirstOrDefault(a => a.Id == videoId);

            if (video.Snippet.ChannelId != BotUser.youtube_channel)
            {
                await FollowupAsync(embed:
                           ($"## Video not found in your channel !\n" +
                           $"- your channel hasn't such video, make sure its your video !")
                           .ToEmbed("Not Found"));
                return;
            }

            var result = await _database.InsertAsync(new YoutuberVideo()
            {
                createdAt = DateTime.Now,
                videoId = videoId,
                user = this.BotUser.Id
            });

            if (result)
            {
                await FollowupAsync(embed:
                       ($"## Request Sent Succesfuly !\n" +
                       $"- Your video verified and sent to developers, we will inform you the result!")
                       .ToEmbed("Operator was succesful", Color.Green));
            }
            else
            {
                await FollowupAsync(embed:
                     ($"## Request Failed !\n" +
                     $"- can't send your request, try again !")
                     .ToEmbed("Operator was not succesful", Color.Red));
            }
        }



    }
}
