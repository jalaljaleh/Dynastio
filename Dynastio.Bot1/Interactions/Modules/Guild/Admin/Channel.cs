using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Dynastio.Bot.Extensions.Discord;
using Discord.WebSocket;
using Newtonsoft.Json;


namespace Dynastio.Bot.Interactions.Modules.Guild.Admin
{
    [EnabledInDm(false)]
    [Group("channel", "server configuration")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class ChannelsModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [Group("overview", "channel overview")]
        [RateLimit(30, 3, RateLimit.RateLimitType.User)]
        public class OverviewModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            [RequireBotPermission(ChannelPermission.ManageChannels)]
            [SlashCommand("slowmode", "Set Slowmode")]
            public async Task slowmode(ITextChannel channel, TimeType time, int value)
            {
                await DeferAsync();
                var slowModeInterval = value * (int)time;
                if (slowModeInterval <= 21600) // api limit
                {
                    await channel.ModifyAsync(a =>
                    {
                        a.SlowModeInterval = slowModeInterval;
                    });
                    await FollowupAsync(embed: $"Slowmode is ready for this channel <#{channel.Id}>".ToWarnEmbed("Initialized"));
                    return;
                }
                await FollowupAsync(embed: "Can not set more than 6 hours".ToWarnEmbed("Discord Error"));
            }
        }
        [RequireDatabase]
        [Group("image-only", "image only channels")]
        public class ImageOnlyModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public GuildService GuildService { get; set; }
            [RequireBotPermission(GuildPermission.ManageMessages)]
            [RequireChannelSlowmode(Program.ImageOnlyChannelsSlowMode)]
            [RateLimit(80, 8, RateLimit.RateLimitType.User)]
            [SlashCommand("add", "Image Channels")]
            public async Task add(ITextChannel channel)
            {
                await DeferAsync();
                if (Context.BotGuild.OnlyImageChannels.Contains(channel.Id))
                {
                    await FollowupAsync(Context.User.Id.ToUserMention(), embed: "The channel is image-channel already.".ToWarnEmbed("Channel Is Image-channel"));
                    return;
                }

                Context.BotGuild.OnlyImageChannels.Add(channel.Id);
                await GuildService.UpdateAsync(Context.BotGuild);
               
                await FollowupAsync(Context.User.Id.ToUserMention(), embed: "The channel added to the image-channels.".ToSuccessfulEmbed("Channel Added"));
            }
            [SlashCommand("list", "Image Channels")]
            public async Task list()
            {
                await DeferAsync();
                var content = string.Join("\n", Context.BotGuild.OnlyImageChannels.Select(a => MentionUtils.MentionChannel(a)).ToArray());
                await FollowupAsync(embed: content.ToEmbed("Image-only channels"));
            }
            [SlashCommand("remove", "Image Channels")]
            public async Task remove(ITextChannel channel)
            {
                await DeferAsync();
                if (Context.BotGuild.OnlyImageChannels.Contains(channel.Id))
                {
                    await FollowupAsync(Context.User.Id.ToUserMention(), embed: "The channel removed from image-channels.".ToWarnEmbed("Channel removed"));
                    Context.BotGuild.OnlyImageChannels.Remove(channel.Id);
                    await GuildService.UpdateAsync(Context.BotGuild);
                    return;
                }
                else
                {
                    await FollowupAsync(Context.User.Id.ToUserMention(), embed: "The channel is not image-channel.".ToSuccessfulEmbed("Channel not found"));
                }
            }
        }

    }
}
