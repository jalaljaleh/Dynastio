using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.Modals;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.buttons
{
    public class AddAccountButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }

        public const string CustomId = "btn.dynastio.accounts.add";
        public static Emoji Emoji => new Emoji("➕");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.accounts.add.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId =CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {

            var modal = new ModalBuilder(userLocale["modal.dynastio.searchplayer.title"], AddAccountModal.CustomId)
               .AddTextInput(new TextInputBuilder(this["account_id"], "id", TextInputStyle.Short, "google:0000000000000000000", 1, 150, true, null))
               .AddTextInput(new TextInputBuilder(this["pincode"], "pincode", TextInputStyle.Short, this["XXX-XXX-XXX"], 11, 11, true, null))
               .AddTextInput(new TextInputBuilder(this["reminder"], "reminder", TextInputStyle.Short, "its a reminder field you can write anything.", 0, 16, false, null))
               .AddTextInput(new TextInputBuilder(this["email"], "email", TextInputStyle.Short, "Gmail, Discord, Facebook, Youtube Channel Address.", 0, 100, false, null))
               .Build();

            await RespondWithModalAsync(modal);

            await ModifyCurrentMessageToInputModeAsync();
        }

    }
}
