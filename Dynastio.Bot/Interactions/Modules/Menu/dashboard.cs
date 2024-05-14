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
using Dynastio.Bot.Services;
using Amazon.Runtime.Internal.Util;

namespace Dynastio.Bot.Interactions.Modules.Menu
{
    [RateLimit(5, 2)]
    [RequireContext(ContextType.Guild)]
    public class DashboardModule : BotInteractionModuleBase, IMenuModule
    {
        public const string CustomId = "btn.menu.dashboard";
        public static Emoji Emoji => new Emoji("➕");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.menu.dashboard.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }

        [SlashCommand("dashboard", "dashboard")]
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
                .WithButton(DynastioModule.GetButton(userLocale), 0)
                .WithButton(SettingsModule.GetButton(userLocale), 0)
                .WithButton(SetupModule.GetButton(userLocale), 0);

            component.WithButton(CancelButton.GetButton(userLocale), 2);

            component = advertisingService.ExploitationAdvertisingButtons(component, 2);

            return true;
        }

        public bool TryCreateEmbeds(out Embed[] embeds)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Dynast.io Bot Dashboard",
                ThumbnailUrl = BotAvatarUrl,
                Color = Color.Green,
                Description =
                "Welcome to Dynast.io Bot Dashboard !\n" +
                "Select a menu to open !",
            }.Build();

            embeds = new Embed[] { embed };
            return true;
        }
    }


}