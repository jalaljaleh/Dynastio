using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
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
using Dynastio.Extenstions;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
{
    public class PrivateServersButton : BotInteractionModuleBase
    {
        public const string CustomId = "btn.dynastio.privateservers";
        public static Emoji Emoji => new Emoji("🔒");
        public static ButtonBuilder GetButton(Locale locale, int serversCount)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.privateservers.label", serversCount],
                Style = ButtonStyle.Secondary,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }

        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }


        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();


            var interaction = Context.Interaction as SocketMessageComponent;

            var privateServers = dynastio.OnlineServers
                ?.Where(a => a.IsPrivate)
                ?.OrderByDescending(a=>a.PlayersCount)
                .Take(30)
                .ToList() 
                
                ?? null;

            if (privateServers is null)
            {
                await ModifyCurrentMessageAsync(embed: "No any online private server found.".ToEmbed("Not Found !", color: Color.Orange));
                return;
            }

            var privateServersContent = privateServers.ToStringTable(new[] { "R", $"{userLocale["server"]}‌", "Link" },
                  a => (privateServers.IndexOf(a) + 1).ToRegularCounter() + ". ",
                  a => $"` {a.Label.Trim().Replace("'", "").TryRemove(20)}.> `",
                  a => $"[**{userLocale["join"]} {a.PlayersCount}/{a.ConnectionsLimit}**]({a.GetDirectLink()})");

             privateServersContent += "\n\n" + advertisingService.GetInlineEmbedDescription(2);

            await ModifyCurrentMessageAsync(embed: privateServersContent.ToEmbed(userLocale["btn.dynastio.privateservers.label", privateServers.Count]));
        }

    }
}
