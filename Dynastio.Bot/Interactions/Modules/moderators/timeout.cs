using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;
using Dynastio.Data;

namespace Dynastio.Bot.Interactions.modules.moderators
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    public class timeoutModule : CustomInteractionModuleBase
    {
        public WebhookService _webhookService { get; set; }
        public DynastioData _dynastioData { get; set; }

        [SlashCommand("mute", "mute a user")]
        public async Task mute(IGuildUser user, TimeType time, int value, string reason, bool warn = false)
        {
            await DeferAsync();

            var time_ = value * (int)time;
            if (time_ > 2419200) // api limit
            {
                await FollowupAsync(embed: "Can not set more than 29 days.".ToEmbed("Api limit", Color.Orange));
                return;
            }

            var timeSpan = TimeSpan.FromSeconds(time_);

            await user.SetTimeOutAsync(timeSpan);

            if( warn )
            {
                var targetUser = await _dynastioData.GetUserAsync(user.Id);

                targetUser.Warns.Add(new Data.UserWarn()
                {
                    Content = reason,
                    CreatedAt = DateTime.UtcNow,
                    SourceId = Context.User.Id
                });

                await _dynastioData.UpdateAsync(targetUser);
            }

            var embed = new EmbedBuilder()
            {
                Description = $"{user.Mention} You have been muted for ` {reason} `.",
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Color = Color.Red,
                Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Duration | Revoke",
                            Value = value + " " + time.ToString() + " | " + (DateTime.UtcNow + timeSpan).ToDiscordUnixTimestampFormat(),
                            IsInline = true
                        },
                          new EmbedFieldBuilder()
                        {
                            Name = "Warn",
                            Value = warn ? "`applied`": "`not applied`",
                            IsInline = true
                        },
                          new EmbedFieldBuilder()
                        {
                            Name = "Moderator",
                            Value = userMention,
                            IsInline = true
                        }
                    }
            }
            .Build();

            await _webhookService
                .LogTimeOutAsync(embed, Context.User)
                .TryAsync();

            await FollowupAsync(
                text: user.Mention,
                embed: embed
                //components: new ComponentBuilder()
                //            .WithButton("Revoke Now", $"btn.mute.revoke:{user.Id}", ButtonStyle.Danger, new Emoji("🔘"))
                //            .Build()
                );
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
