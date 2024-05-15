using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio.subscription
{
    public class BadgesRoleSettingsButton : BotInteractionModuleBase
    {

        public const string CustomId = "btn.subscription.rank";
        public static Emoji Emoji => new Emoji("➕");
        public static ButtonBuilder GetButton()
        {
            return new ButtonBuilder()
            {
                Label = "Enable Badges Module",
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await RespondAsync(ephemeral: true, text: "Your guild subscription is not activated !");
        }

    }
}
