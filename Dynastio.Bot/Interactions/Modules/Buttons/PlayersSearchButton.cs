
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Enums;
using Dynastio.Bot.Interactions.Modules.Modals;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.buttons
{
    public class PlayersSearchButton : BotInteractionModuleBase
    {

        public const string CustomId = "btn.dynastio.players.search";
        public static Emoji Emoji => new Emoji("🔍");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.players.search.label"],
                Style = ButtonStyle.Primary,
                Emote = PlayersSearchButton.Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = PlayersSearchButton.CustomId
            };
        }

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            var modal = new ModalBuilder(userLocale["modal.dynastio.searchplayer.title"], SearchPlayerModal.CustomId)

             .AddTextInput(new TextInputBuilder(userLocale["nickname"], "nickname", TextInputStyle.Short, "Jaleh", 0, 16, false, null))
             .AddTextInput(new TextInputBuilder(userLocale["team"], "team", TextInputStyle.Short, "Aliens", 0, 16, false, null))
             .AddTextInput(new TextInputBuilder(userLocale["server"], "server", TextInputStyle.Short, "London-01", 0, 20, false, null))
             .AddTextInput(new TextInputBuilder(userLocale["score"], "score", TextInputStyle.Short, "1000", 0, 20, false, null))
             .AddTextInput(new TextInputBuilder(userLocale["level"], "level", TextInputStyle.Short, "10", 0, 5, false, null))
             .Build();

            await RespondWithModalAsync(modal);

            await ModifyCurrentMessageToInputModeAsync();
        }

      
    }
}
