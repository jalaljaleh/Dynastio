using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
{
    public class VersionButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }

        public const string CustomId = "btn.dynastio.version";
        public static Emoji Emoji => new Emoji("🔸");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.version.title"],
                Style = ButtonStyle.Primary,
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
            await DeferAsync();


            var version = dynastio.Version;
            var message = await ModifyCurrentMessageAsync(
                embed:
                $"Current version is Is **{version.CurrentVersion}** [Download]({version.DownloadUrl})"
                .ToEmbed("Version", Context.Client.CurrentUser.GetAvatarUrl()));
        }

    }
}
