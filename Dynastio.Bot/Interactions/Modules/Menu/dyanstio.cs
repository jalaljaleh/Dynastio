using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Buttons;
using Dynastio.Bot.Interactions.Modules.Buttons.bot;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.Menu
{

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    [RequireBotPermission(ChannelPermission.AttachFiles)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class DynastioModule : BotInteractionModuleBase, IMenuModule
    {
        public DynastioApi _dynastio { get; set; }

        public const string CustomId = "btn.menu.dynastio";
        public static Emoji Emoji => new Emoji("⚔️");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.menu.dynastio.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }

        [SlashCommand("dynastio", "dynastio menu")]
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
        public bool TryCreateEmbeds(out Embed[] embeds)
        {
            embeds = new Embed[] { };
            //var embed = new EmbedBuilder()
            //{
            //    Title = "Prerequisite Required",
            //    Description = "A prerequisite is required. After solving the prerequisite, you can use this command.",
            //    ThumbnailUrl = BotAvatarUrl,
            //}.Build();
            return true;
        }
        public bool TryCreateComponents(out ComponentBuilder component)
        {
            component = new ComponentBuilder();

            if (BotUser.IsAccountConnected is false)
            {
                component.WithButton(ConnectAccountButton.GetButton(userLocale), 0);
                component.WithButton(CancelButton.GetButton(userLocale), 0);

                return false;
            }

            component
                .WithButton(PlayersButton.GetButton(userLocale, _dynastio.OnlinePlayers.Count), 0)
                .WithButton(TeamsButton.GetButton(userLocale, _dynastio.OnlinePlayers.GroupBy(a => a.Team).Count()), 0)
                .WithButton(PrivateServersButton.GetButton(userLocale, _dynastio.OnlineServers.Where(a => a.IsPrivate).Count()), 0)
                .WithButton(PlayersSearchButton.GetButton(userLocale), 0)

                .WithButton(ProfileButton.GetButton(userLocale, BotUser.Accounts.Any()), 1)

                .WithButton(LeaderboardButton.GetButton(userLocale), 2)
                .WithButton(VersionButton.GetButton(userLocale), 2);

            component.WithButton(CancelButton.GetButton(userLocale), 3);

            component = advertisingService.ExploitationAdvertisingButtons(component, 3, 4);

            return true;
        }




    }
}
