using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Enums;
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
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }
        public AdvertisingService adsService { get; set; }

        public const string CustomId = "btn.dynastio.players.search";
        public static Emoji Emoji => new Emoji("🔍");

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
         //   await RespondWithModalAsync();

            await ModifyCurrentMessageToInputModeAsync();
        }

    }
}
