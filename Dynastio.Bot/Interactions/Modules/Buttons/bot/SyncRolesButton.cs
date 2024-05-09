using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.Modals;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.buttons
{
    public class SyncRolesButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }

        public const string CustomId = "btn.guild.sync.roles";
        public static Emoji Emoji => new Emoji("⚖️");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.guild.sync.roles.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }
        [RequireContext(ContextType.Guild)]
        [RequireComponentMessageMention]
        [RateLimit(500, 1)]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await this.DeferAsync();

            await userService.SyncUserRolesAsync(BotGuild,BotUser,Context.User as IGuildUser);
           
            await this.CloseMenuAsync();
        }

    }
}
