using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Guild.Admin
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class Mute : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [RateLimit(5, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("mute", "mute a user")]
        public async Task mute(IGuildUser user, TimeType time, int value)
        {
            await DeferAsync();
            var time_ = value * (int)time;
            if (time_ > 2419200) // api limit
            {
                await FollowupAsync(embed: "Can not set more than 6 hours".ToWarnEmbed("Discord Error"));
                return;
            }
            var timeSpan = TimeSpan.FromSeconds(time_);
            await user.SetTimeOutAsync(timeSpan);
            await FollowupAsync(embed: $"User <@{user.Id}> muted until {(DateTime.UtcNow + timeSpan).ToDiscordUnixTimestampFormat()} by {Context.User.Id.ToUserMention()}.".ToSuccessfulEmbed(user.Username + " Muted"));
        }
    }
}
