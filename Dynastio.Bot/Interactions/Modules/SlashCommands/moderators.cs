using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    public class ModeratorModule : BotInteractionModuleBase
    {
        const long max = 24191999999992;

        [SlashCommand("mute", "mute users")]
        public async Task mute(IGuildUser user, int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            await DeferAsync();

            var timeout = DateTime.UtcNow
                            .AddDays(days)
                            .AddHours(hours)
                            .AddMinutes(minutes)
                            .AddSeconds(seconds) - DateTime.UtcNow;

            if (timeout.Ticks > max)
            {
                await FollowupAsync(userMention, embed: $"Time `{timeout.Days} days, {timeout.Hours} hours, {timeout.Minutes} minutes, {timeout.Seconds} seconds, ` can not be more than {TimeSpan.FromTicks(max).ToString("dd\\:hh\\:mm\\:ss")} days.".ToEmbed("Discord Limits"));
                return;
            }
            try
            {
                await user.SetTimeOutAsync(timeout);

                await FollowupAsync(userMention, embed:
                    $"{user.Mention} muted for `{timeout.Days} days, {timeout.Hours} hours, {timeout.Minutes} minutes and {timeout.Seconds} seconds ` and will be unmuted {DateTime.UtcNow.AddTicks(timeout.Ticks).UnixTimestampDiscordFormat()}"
                    .ToEmbed("User Muted", user.TryGetAvatarUrl(), color: Color.Orange));
            }
            catch
            {
                await FollowupAsync(userMention, embed: $"unkown error...".ToEmbed("error"));
            }

        }

    }
}
