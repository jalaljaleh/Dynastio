using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.@shared_buttons
{
    public class CancelButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }
        

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            await CloseMenuAsync();
        }

        public const string CustomId = "btn.cancel";
        public static Emoji Emoji => new Emoji("✖️");
        public static ButtonBuilder GetButton(Locale locale, bool safeCancel = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["cancel"],
                Style = safeCancel ? ButtonStyle.Success : ButtonStyle.Danger,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }

    }
}
