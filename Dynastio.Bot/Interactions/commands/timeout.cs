using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.Modules
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [DefaultMemberPermissions(GuildPermission.ModerateMembers)]
    public class timeoutModule : CustomInteractionModuleBase
    {
        [SlashCommand("mute", "mute a user")]
        public async Task mute(IGuildUser user, TimeType time, int value,string reason = "no reason provided")
        {
            await DeferAsync();
            var time_ = value * (int)time;
            if (time_ > 2419200) // api limit
            {
                await FollowupAsync(embed: "Can not set more than 6 hours".ToEmbed("Api limit", Color.Orange));
                return;
            }
            var timeSpan = TimeSpan.FromSeconds(time_);
            await user.SetTimeOutAsync(timeSpan);
            await FollowupAsync(embed: $"User <@{user.Id}> muted until {(DateTime.UtcNow + timeSpan).ToDiscordUnixTimestampFormat()} by {Context.User.Id.ToUserMention()} for ` {reason} `.".ToEmbed(user.Username + " Muted"));
        }

    }
    public enum TimeType
    {
        None = 0,
        Secound = 1,
        Minute = 60,
        Hour = 3600,
        Day = 86400,
        Week = 604800,
        Month = 2419200,
        Year = 29030400
    }
}
