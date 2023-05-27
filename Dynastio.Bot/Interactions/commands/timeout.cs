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
        public async Task mute(IGuildUser user, TimeType time, int value, string reason = "no reason provided")
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
            await FollowupAsync(
                text: user.Mention,
                embed: new EmbedBuilder()
                {
                    Description = $"You have been muted by {Context.User.Id.ToUserMention()}..",
                    ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                    Color = Color.DarkRed,
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Duration",
                            Value = value + " " + time.ToString(),
                            IsInline = true
                        },
                         new EmbedFieldBuilder()
                        {
                            Name = "Revoke",
                            Value =  (DateTime.UtcNow + timeSpan).ToDiscordUnixTimestampFormat(),
                            IsInline = true
                        },
                          new EmbedFieldBuilder()
                        {
                            Name = "Moderator",
                            Value = userMention,
                            IsInline = true
                        }, new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = false
                        },
                    }
                }.Build());
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
