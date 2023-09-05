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

        private static List<YoutuberVideo> _videos = new();

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireRole(480954902005415937)]
        [SlashCommand("promote-video-list", " promote list requests!")]
        public async Task promotelist()
        {
            await DeferAsync();

            _videos = await _database.GetYoutuberVideosAsync();

            if (_videos.Any() is false)
            {
                await FollowupAsync(userMention, embed: "no any video found, try later !".ToEmbed("video not found !"));
                return;
            }

            await FollowupAsync(userMention, embed: new EmbedBuilder()
            {
                Title = "Promote Videos Menu",
                Description = "Here you can manage promoted videos that requested by dynastio youtubers !",
                Fields = new List<EmbedFieldBuilder>()
                            { new EmbedFieldBuilder()
                            .WithName("Requests Count")
                            .WithValue(_videos.Count)
                            .WithIsInline(true)
                            }
            }.Build(),
              components: new ComponentBuilder()
            .WithButton("Start", $"btn.youtubers-video:start:{_videos.FirstOrDefault().videoId}", ButtonStyle.Primary)
            .Build());
        }
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireRole(480954902005415937)]
        [ComponentInteraction("btn.youtubers-video:*:*")]
        public async Task promotevideo(string action, string videoid)
        {
            var video = _videos.FirstOrDefault(a => a.videoId == videoid);
            if (_videos.Any() is false || video is null)
            {
                await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
                await promotelist();
                return;
            }

            if (Context.Interaction.HasResponded is false)
                await DeferAsync();

            switch (action)
            {
                case "start":
                    await postVideo();
                    break;
                case "cancel":
                    await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
                    await FollowupAsync("Video Promote Requests Closed !");
                    break;

                case "promoted":
                case "not_promoted":

                    await sendMessageToVideoOwner($"Your video{(action == "promoted" ? "" : " not")} confirmed to be promoted.")
                        .TryAsync();
                    await _database.DeleteAsync(video);
                    _videos.Remove(video);
                    await promotevideo("start", _videos.FirstOrDefault().videoId);
                    break;

                case "skip":
                    _videos.Remove(video);
                    await promotevideo("start", _videos.FirstOrDefault().videoId);
                    break;
            }
            async Task postVideo()
            {
                await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
                await FollowupAsync(userMention + " | " + video.GetUrl() + " \n" + video.GetUrl().ToMarkdown(),
                          components: new ComponentBuilder()
                        .WithButton("Promoted", $"btn.youtubers-video:promoted:{videoid}", ButtonStyle.Success)
                        .WithButton("Not Promoted", $"btn.youtubers-video:not_promoted:{videoid}", ButtonStyle.Danger)
                        .WithButton("Skip >>", $"btn.youtubers-video:skip:{videoid}", ButtonStyle.Primary)
                        .WithButton("Promote 3 days", $"btn.youtubers-video:promote3:{videoid}", ButtonStyle.Primary, disabled: true, row: 1)
                        .WithButton("Promote 5 days", $"btn.youtubers-video:promote5:{videoid}", ButtonStyle.Primary, disabled: true, row: 1)
                        .WithButton("Promote 10 days", $"btn.youtubers-video:promote10:{videoid}", ButtonStyle.Primary, disabled: true, row: 1)
                        .WithButton("Custom Duration", $"btn.youtubers-video:promotecustom:{videoid}", ButtonStyle.Primary, disabled: true, row: 1)
                        .WithButton("Cancel", $"btn.youtubers-video:cancel:{videoid}", ButtonStyle.Danger, row: 2)
                        .Build());
            }
            async Task sendMessageToVideoOwner(string text)
            {
                var videoOwner = await Context.Client.GetUserAsync(video.user);
                await videoOwner.SendMessageAsync(
                    text +
                    $"\nVideo: {video.GetUrl()}");
            };
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
                await sentDenyReasonTouser();
                await deleteRequest();
                return;
            }
            async Task sentDenyReasonTouser()
            {
                await user.SendMessageAsync("" +
                   "Your request for connection your youtube channel denied by developers.\n" +
                   "## This happens if:" +
                   "- Your youtube channel is not connected to your discord account connection.\n" +
                   "- This is not the official server.\n" +
                   "- Your account is banned.\n" +
                   "- The channel added by someone else.\n" +
                   "- You have a connected channel already." +
                   "")
                   .TryAsync();
            }
            var checkUsers = await _database.GetUserByYoutubeChannelIdAsync(channel);
            if (checkUsers is not null)
            {
                await FollowupAsync(embed:
                                  ($"## Channel added by someone else already\n" +
                                   $"<@{checkUsers}> added this channel alrady, if its your channel but someone else added it, infrom us by creating a ticket.")
                                   .ToEmbed("Access Denied"));

                await sentDenyReasonTouser();
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

                await sentDenyReasonTouser();

                await deleteRequest();
                return;
            }


            targetUser.youtube_channel = channel;

            await _userService.UpdateAsync(targetUser);

            await user.SendMessageAsync($"" +
                $"## Youtube channel verified\n" +
                $"- Your request for connecting your youtube channel to your bot account accepted by developers.\n" +
                $"https://www.youtube.com/{channel}\n" +
                $"")
                 .TryAsync();

            await deleteRequest();

            async Task deleteRequest()
            {
                await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
            }
        }

        //[SlashCommand("promote-channel-video", "promote your dynastio video !")]
        //public async Task promotechannelvideo()
        //{
        //    await DeferAsync();

        //    if (string.IsNullOrEmpty(BotUser.youtube_channel))
        //    {
        //        await FollowupAsync(embed:
        //            ($"## You have to connect your channel to your bot account first.\n" +
        //            $"- connect your youtube channel to your discord account first from **Discord Connections**.`\n" +
        //            $"- then use this command to coonect your channel to the bot `/connect-youtube-channel`\n" +
        //            $"- wait for developers to confirm your request.\n" +
        //            $"after doing the steps, we will infrom you the result !")
        //            .ToEmbed("Channel not found", Color.Orange));
        //        return;
        //    }

        //}
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
                           .ToEmbed("", Color.Orange));
                return;
            }

            var video = videos.Items.FirstOrDefault(a => a.Id == videoId);

            if (video.Snippet.ChannelId != BotUser.youtube_channel)
            {
                await FollowupAsync(embed:
                           ($"## Video not found in your channel !\n" +
                           $"- your channel hasn't such video, make sure its your video !")
                           .ToEmbed("", thumbnailUrl: video?.Snippet?.Thumbnails?.Default__?.Url ?? ""));
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
                       $"- Your video verified and sent to developers, we will inform you the result !")
                       .ToEmbed("", thumbnailUrl: video?.Snippet?.Thumbnails?.Default__?.Url ?? "", color: Color.Green));
            }
            else
            {
                await FollowupAsync(embed:
                     ($"## Request Failed !\n" +
                     $"- can't send your request, try again !")
                     .ToEmbed("", Color.Red));
            }
        }



    }
}
