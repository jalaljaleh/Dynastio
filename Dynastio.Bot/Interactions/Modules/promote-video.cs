using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Data;
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
        public DynastioData _dynastioData { get; set; }
        public InternetService _internetService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public YoutubeService _youtubeService { get; set; }
        public UserService _userService { get; set; }
        public GuildService _guildService { get; set; }


        [RequireDeveloper]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [ComponentInteraction("btn.promote-video:*:*:*")]
        public async Task promotevideo(string action, ulong requester, string videoid)
        {
            await _userService.SendMessageAsync(requester,
                $"# Promote Video\n" +
                $"- Your request to promote your youtube video**{(action == "promoted" ? " " : " not ")}confirmed** to be promoted.\n" +
                $"{YoutubeService.GetUrlFromVideoId(videoid)}")
                .TryAsync();

            await (Context.Interaction as SocketMessageComponent).Message.DeleteAsync();
        }
        [SlashCommand("promote-video", "promote your dynastio video !", false, RunMode.Sync)]
        public async Task promote([Autocomplete(typeof(AutoCompeleteYoutuberVideos))] string videoId)
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

            string videoUrl = YoutubeService.GetUrlFromVideoId(videoId);
            var confirmChannel = Context.Guild.GetTextChannel(_guildService.GetChannelId(Channels.GuildChannelType.ConfirmPromoteVideos));

            var result = await confirmChannel.SendMessageAsync(
                $"✦•··························• Dynast.io •··························•✦\r\n" +
                $"# {video.Snippet?.Title.TryRemove(40) ?? "Title not found"} + {(video.Snippet.PublishedAt.HasValue ? video.Snippet.PublishedAt.Value.ToDiscordUnixTimestampFormat() : "Unknown")}\n" +
                $"- Description: {video.Snippet?.Description.TryRemove(3000).ToMarkdown() ?? "No Description"}\n" +
                $"- Requester: {userMention}\n" +
                $"- Url: {videoUrl}\n" +
                $"{videoUrl.ToMarkdown()}\n" +
                $"",

                components: new ComponentBuilder()
            .WithButton("Promoted", $"btn.promote-video:promoted:{Context.User.Id}:{videoId}", ButtonStyle.Success)
            .WithButton("Not Promoted", $"btn.promote-video:not-promoted:{Context.User.Id}:{videoId}", ButtonStyle.Danger)
            .WithButton("Promote 3 days", $"*3*", ButtonStyle.Primary, disabled: true, row: 1)
            .WithButton("Promote 5 days", $"*4*", ButtonStyle.Primary, disabled: true, row: 1)
            .WithButton("Custom Duration", $"*c*", ButtonStyle.Success, disabled: true, row: 1)
            .WithButton("Open in Browser", null, ButtonStyle.Link, null, videoUrl, row: 2)
              .Build())
                .TryAsync();


            if (result.isSuccesful)
            {
                await FollowupAsync(
                    embeds: new Embed[]
                    {
                       ($"## Request Sent Succesfuly !\n" +
                       $"- Your video verified and sent to developers, we will inform you the result !")
                       .ToEmbed("", thumbnailUrl: video?.Snippet?.Thumbnails?.Default__?.Url ?? "", color: Color.Green),

                       new EmbedBuilder(){Description= result.result.Content}.Build()
                    });
            }
            else
            {
                await FollowupAsync(embed:
                     ($"## Request Failed !\n" +
                     $"- can't send your request, try again !")
                     .ToEmbed("", Color.Red));
            }
        }


        public class AutoCompeleteYoutuberVideos : AutocompleteHandler
        {
            public YoutubeService youtubeService { get; set; }
            public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
            {
                string match = autocompleteInteraction.Data.Current.Value.ToString();
                var videos = await youtubeService.GetAllChannelVideos((context as CustomSocketInteractionContext).BotUser.youtube_channel);
                var result = new List<AutocompleteResult>();
                foreach (var v in videos.Where(a => a.Snippet.Title.Contains(match)).Take(25))
                {
                    result.Add(new AutocompleteResult()
                    {
                        Name = v.Snippet.Title.TryRemove(80),
                        Value = v.Id
                    });
                }
                return await Task.FromResult(AutocompletionResult.FromSuccess(result.Take(25)));
            }
        }



        [RequireDeveloper]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("disconnect-youtube-channel", "disconnect your youtube channel !")]
        public async Task connectchannel(IUser target)
        {
            await DeferAsync();
            var targetUser = await _dynastioData.GetUserAsync(target.Id, false);
            if (targetUser != null)
            {
                targetUser.youtube_channel = null;
                await _dynastioData.UpdateAsync(targetUser);
            }
            await FollowupAsync(embed: "Operator was succesful".ToEmbed("channel disconnected from the user profile."));
        }
        [SlashCommand("connect-youtube-channel", "connect your youtube channel !", false, RunMode.Sync)]
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

            var checkUsers = await _dynastioData.GetUserByYoutubeChannelIdAsync(channelId);
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


        [ComponentInteraction("btn.connectyoutubechannel:*:*:*", false, RunMode.Sync)]
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
                   "## This happens if:\n" +
                   "- Your youtube channel is not connected to your discord account connection.\n" +
                   "- This is not the official server.\n" +
                   "- Your account is banned.\n" +
                   "- The channel added by someone else.\n" +
                   "- You have a connected channel already.\n" +
                   "## `You can create a ticket for support anytime.`")
                   .TryAsync();
            }
            var checkUsers = await _dynastioData.GetUserByYoutubeChannelIdAsync(channel);
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

            var targetUser = await _dynastioData.GetUserAsync(userId);
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

            await _dynastioData.UpdateAsync(targetUser);

            try
            {
                // youtuber ach role
                await Context.Guild.GetUser(userId).AddRoleAsync(1139588083373854831);
                await Task.Delay(100);
            }
            catch
            {

            }
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




    }
}
