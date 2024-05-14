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
using Dynastio.Bot.Interactions.Modules.Buttons.bot;

namespace Dynastio.Bot.Interactions.Modules.Menu
{
    [RateLimit(5, 2)]
    [RequireContext(ContextType.Guild)]
    public class SettingsModule : BotInteractionModuleBase, IMenuModule
    {
        public const string CustomId = "btn.menu.settings";
        public static Emoji Emoji => new Emoji("🛠️");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.menu.settings.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }

        [SlashCommand("settings", "settings menu")]
        public async Task SlashCommandAsync()
        {
            await ExecuteAsync();
        }

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ButtonAsync()
        {
            await ExecuteAsync();
        }

        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var componentResult = TryCreateComponents(out ComponentBuilder component);
            var embedsResult = TryCreateEmbeds(out Embed[] embeds);

            if (Context.Interaction is SocketMessageComponent)
                await ModifyCurrentMessageAsync(userMention, embeds: embeds, components: component.Build());
            else
                await FollowupAsync(
                    text: TryCreateTextContent(),
                    embeds: embeds,
                    components: component.Build());
        }

        public string TryCreateTextContent()
        {
            var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.MessageContent, 1).FirstOrDefault();
            return Context.User.Mention + " | " + contentAdvertises?.GetEmbedLink();
        }
        public bool TryCreateComponents(out ComponentBuilder component)
        {
            component = new ComponentBuilder();

            component
                .WithButton(RankButton.GetButton(userLocale), 0)
                .WithButton(SyncRolesButton.GetButton(userLocale, !BotGuild.HasSubscription()), 0)

                .WithButton(AddAccountButton.GetButton(userLocale, BotUser.Accounts.Count > 19), 1);

            if (BotUser.GetMainAccount() == null)
                component.WithButton(ConnectAccountButton.GetButton(userLocale), 1);

            component.WithButton(CancelButton.GetButton(userLocale), 2);

            component = advertisingService.ExploitationAdvertisingButtons(component, 2);

            return true;
        }
        public bool TryCreateEmbeds(out Embed[] embeds)
        {
            var profile = Context.BotUser.GetServerProfile(Context.Guild.Id);
            var fileds = new List<EmbedFieldBuilder>()
            {
                    new EmbedFieldBuilder()
                .WithName("Level")
                .WithValue($"level: ` {profile.Level} `")
                .WithIsInline(true),

                    new EmbedFieldBuilder()
                .WithName("XP")
                .WithValue($"xp: ` {profile.Xp} `")
                .WithIsInline(true),

                  new EmbedFieldBuilder()
                .WithName("Ranking Profiles")
                .WithValue( $"Server Profiles: ` {Context.BotUser.GuildProfiles.Count} `")
                .WithIsInline(true),

                 new EmbedFieldBuilder()
                .WithName($"{BotUser.Accounts.Count} Accounts")
                .WithValue(BotUser.Accounts.Any() ? string.Join("\n", BotUser.Accounts.Select(a=> "- "+ a.Reminder)) : "` No Account `")
                .WithIsInline(true),

                   new EmbedFieldBuilder()
                .WithName("Main Account")
                .WithValue(BotUser.IsAccountConnected ? $"` {Context.BotUser.GetMainAccount().Reminder} `" : "` Not Connected `")
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
                .WithValue(string.IsNullOrEmpty(BotUser.youtube_channel) ? "` Not Linked `" : "### "+BotUser.youtube_channel.ToYoutubeChannel().ToMarkdownLink("Youtube Channel"))
                .WithIsInline(true),
            };
            var embed = new EmbedBuilder()
            {
                Title = Context.User.GlobalName,
                ThumbnailUrl = Context.User.TryGetAvatarUrl(),
                Color = (Context.User as IGuildUser).TryGetRoleColor(),
                Description =
                "Welcome to your personal menu !\n" +
                "Here you can manage and view your account details & settings !",
                Fields = fileds
            }.Build();

            embeds = new Embed[] { embed };
            return true;
        }
    }
}