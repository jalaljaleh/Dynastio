using Discord.Interactions;
using Discord;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Modules.Modals;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Interactions.Modules.buttons;
using Discord.WebSocket;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;

namespace Dynastio.Bot.Interactions.Modules.Buttons
{
    public class MenuMeButton : BotInteractionModuleBase
    {

        public const string CustomId = "btn.menu.me";
        public static Emoji Emoji => new Emoji("🛠️");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.menu.me.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var result = TryGetComponents(out ComponentBuilder component);

            if (Context.Interaction is SocketMessageComponent)
                await ModifyCurrentMessageAsync(userMention, embed: GetEmbedBuilder(result), components: component.Build());
            else
                await FollowupAsync(
                    text: userMention,
                    embed: GetEmbedBuilder(result),
                    components: component.Build());
        }

        public Embed GetEmbedBuilder(bool componentResult)
        {
            var profile = Context.BotUser.GetRankingProfile(Context.Guild.Id);
            var fileds = new List<EmbedFieldBuilder>()
            {
                    new EmbedFieldBuilder()
                .WithName("Level")
                .WithValue(profile.Level)
                .WithIsInline(true),

                    new EmbedFieldBuilder()
                .WithName("XP")
                .WithValue(profile.Xp)
                .WithIsInline(true),

                  new EmbedFieldBuilder()
                .WithName("Ranking Profiles")
                .WithValue( $"Profiles: ` {Context.BotUser.GuildProfiles.Count} `")
                .WithIsInline(true),

                 new EmbedFieldBuilder()
                .WithName($"{BotUser.Accounts.Count} Accounts")
                .WithValue(BotUser.Accounts.Any() ? string.Join(", ", BotUser.Accounts.Select(a=> BotUser.Accounts.IndexOf(a) +". "+ a.Reminder)) : "` No Account `")
                .WithIsInline(true),

                   new EmbedFieldBuilder()
                .WithName("Main Account")
                .WithValue(BotUser.IsAccountConnected ? $"Connected ` {Context.BotUser.GetMainAccount().Reminder} `" : "` Not Connected `")
                .WithIsInline(true),

                new EmbedFieldBuilder()
                .WithName("Last Update")
                .WithValue(Context.BotUser.LastUpdateTime.UnixTimestampDiscordFormat())
                .WithIsInline(true),

                new EmbedFieldBuilder()
                .WithName("Joined In")
                .WithValue((Context.User as IGuildUser).JoinedAt.HasValue ? (Context.User as IGuildUser).JoinedAt.Value.UtcDateTime.UnixTimestampDiscordFormat() : "` no data `")
                .WithIsInline(true),

                new EmbedFieldBuilder()
                .WithName("Youtube")
                .WithValue(string.IsNullOrEmpty(BotUser.youtube_channel) ? "` Not Linked `" : BotUser.youtube_channel.ToYoutubeChannel().ToMarkdownLink("Youtube"))
                .WithIsInline(true),
            };
            return new EmbedBuilder()
            {
                Title = Context.User.GlobalName,
                ThumbnailUrl = Context.User.TryGetAvatarUrl(),
                Color = (Context.User as IGuildUser).TryGetRoleColor(),
                Description = "Welcome to your personal menu !",
                Fields = fileds
            }.Build();
        }
        private bool TryGetComponents(out ComponentBuilder component)
        {
            component = new ComponentBuilder();

            component
                .WithButton(RankButton.GetButton(userLocale), 0)
                .WithButton(SyncRolesButton.GetButton(userLocale, !BotGuild.HasSubscription()), 0)

                .WithButton(AddAccountButton.GetButton(userLocale, BotUser.Accounts.Count > 19), 1);

            component.WithButton(CancelButton.GetButton(userLocale), 2);

            component = advertisingService.ExploitationAdvertisingButtons(component, 2);

            return true;
        }

    }
}