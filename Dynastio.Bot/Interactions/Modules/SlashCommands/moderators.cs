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
            await DeferAsync(true);

            if (time.Ticks > max)
            {
                await FollowupAsync(
                    text: userMention,
                    embed: $"Time {(DateTime.UtcNow + time).UnixTimestampDiscordFormat()} can not be more than {TimeSpan.FromTicks(max).ToString("dd\\:hh\\:mm\\:ss")} days.".ToEmbed("Discord Limits"));
                return;
            }
            try
            {
                await user.SetTimeOutAsync(time);
                await FollowupAsync(text: userMention, embed: new EmbedBuilder()
                {
                    Title = "Successful Command",
                    Description = $"Sir, this bad user have been muted successfuly 😄 !",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Duration").WithIsInline(true)
                        .WithValue((DateTime.UtcNow + time).UnixTimestampDiscordFormat()),

                          new EmbedFieldBuilder()
                        .WithName("Reason").WithIsInline(true)
                        .WithValue(reason),
                    },
                    ThumbnailUrl = user.TryGetAvatarUrl(),
                    Color = Color.Green,
                    Author = new EmbedAuthorBuilder() { Name = user.GlobalName, IconUrl = user.TryGetAvatarUrl() }
                }.Build());

                await Context.Channel.SendMessageAsync(text: user.Mention, embed: new EmbedBuilder()
                {
                    Title = "You Are Muted",
                    Description = $"You have been muted for {(DateTime.UtcNow + time).ToRelative()} !",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Duration").WithIsInline(true)
                        .WithValue((DateTime.UtcNow + time).UnixTimestampDiscordFormat()),

                          new EmbedFieldBuilder()
                        .WithName("Reason").WithIsInline(true)
                        .WithValue($"**{reason}**"),
                    },
                    ThumbnailUrl = Global.Resource.ModeratorUrl,
                    Color = Color.Red,
                    Author = new EmbedAuthorBuilder() { Name = "Dynast.io Moderators", IconUrl = BotAvatarUrl }
                }.Build());
            }
            catch
            {
                await FollowupAsync(text: userMention, embed: $"unkown error...".ToEmbed("error"));
            }

        }
        [SlashCommand("warn", "warn users")]
        public async Task warn(IGuildUser user, string reason = "no reason provided !")
        {
            await RespondAsync(ephemeral: true, text: userMention, embed: new EmbedBuilder()
            {
                Title = "Successful Warn",
                Description = $"Sir, this bad user have been warned successfuly 😄 !",
                Fields = new List<EmbedFieldBuilder>()
                    {
                          new EmbedFieldBuilder()
                        .WithName("Reason").WithIsInline(true)
                        .WithValue(reason),
                    },
                ThumbnailUrl = user.TryGetAvatarUrl(),
                Color = Color.Green,
                Author = new EmbedAuthorBuilder() { Name = user.GlobalName, IconUrl = user.TryGetAvatarUrl() }
            }.Build());

            await Context.Channel.SendMessageAsync(text: user.Mention, embed: new EmbedBuilder()
            {
                Title = $"{user.DisplayName} You got a warn !",
                Description = $"{user.GlobalName} Hey, Be carefull, you have been warned !",
                Fields = new List<EmbedFieldBuilder>()
                    {
                          new EmbedFieldBuilder()
                        .WithName("Reason").WithIsInline(true)
                        .WithValue($"**{reason}**"),
                    },
                ThumbnailUrl = Global.Resource.ModeratorUrl,
                Color = Color.Orange,
                Author = new EmbedAuthorBuilder() { Name = "Dynast.io Moderators", IconUrl = BotAvatarUrl }
            }.Build());
        }
    }
}
