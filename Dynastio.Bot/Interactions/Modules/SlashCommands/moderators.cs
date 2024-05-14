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
        public async Task mute(IGuildUser user, TimeSpan time, string reason = "no reason provided !")
        {

            if (time.Ticks > max)
            {
                await FollowupAsync(ephemeral: true, text: userMention, embed: $"Time {(DateTime.UtcNow + time).UnixTimestampDiscordFormat()} can not be more than {TimeSpan.FromTicks(max).ToString("dd\\:hh\\:mm\\:ss")} days.".ToEmbed("Discord Limits"));
                return;
            }
            try
            {
                await user.SetTimeOutAsync(time);

                await FollowupAsync(text: userMention, embed:$"".ToEmbed("User Muted", user.TryGetAvatarUrl(), color: Color.Orange));
            }
            catch
            {
                await FollowupAsync(ephemeral: true, text: userMention, embed: $"unkown error...".ToEmbed("error"));
            }

        }

    }
}
