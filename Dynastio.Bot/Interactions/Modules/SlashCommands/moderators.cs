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
        const long max = 2419200;
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
        [SlashCommand("mute", "mute users")]
        public async Task mute(IGuildUser user, TimeType time, int value, string reason = "no reason provided !")
        {
            await DeferAsync(true);

            int time_ = value * (int)time;
            if (time_ > 2419200) // api limit
            {
                await FollowupAsync(embed: "Can not set more than 29 days.".ToEmbed("Api limit", Color.Orange));
                return;
            }
            var timeSpan = TimeSpan.FromSeconds(time_);

            await user.SetTimeOutAsync(timeSpan);

            await FollowupAsync(text: userMention, embed: new EmbedBuilder()
            {
                Title = "Successful Command",
                Description = $"Sir, this bad user have been muted successfuly 😄 !",
                Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Duration").WithIsInline(true)
                        .WithValue((DateTime.UtcNow + timeSpan).UnixTimestampDiscordFormat()),

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
                Description = $"You have been muted for {(DateTime.UtcNow + timeSpan).ToRelative()} !",
                Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Duration").WithIsInline(true)
                        .WithValue((DateTime.UtcNow + timeSpan).UnixTimestampDiscordFormat()),

                          new EmbedFieldBuilder()
                        .WithName("Reason").WithIsInline(true)
                        .WithValue($"**{reason}**"),
                    },
                ThumbnailUrl = Global.Resource.ModeratorUrl,
                Color = Color.Red,
                Author = new EmbedAuthorBuilder() { Name = "Dynast.io Moderators", IconUrl = BotAvatarUrl }
            }.Build());

        }
        [SlashCommand("warn", "warn users")]
        public async Task warn(IGuildUser user, string reason = "no reason provided !")
        {
            var profile = BotUser.GetServerProfile(Context.Guild.Id);
            profile.Warns += 1;
            await UpdateBotUserAsync();

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
                Description = $"{user.GlobalName} you have been warned for **{reason}** !",
                Fields = new List<EmbedFieldBuilder>()
                    {
                          new EmbedFieldBuilder()
                        .WithName("Warns").WithIsInline(true)
                        .WithValue($"` {profile.Warns} ` Warns"),
                    },
                ThumbnailUrl = Global.Resource.ModeratorUrl,
                Color = Color.Orange,
                Author = new EmbedAuthorBuilder() { Name = "Dynast.io Moderators", IconUrl = BotAvatarUrl }
            }.Build());
        }
    }
}
