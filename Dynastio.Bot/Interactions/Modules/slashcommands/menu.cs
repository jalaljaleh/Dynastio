using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{
    public class MenuModule : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }
        public AdvertisingService adsService { get; set; }

        [SlashCommand("menu", "dynastio menu")]
        [RateLimit(6, 1)]
        public async Task menu()
        {
            await DeferAsync();

            List<Database.Advertise> advertise = new();

            var contentAdvertises = adsService.ExploitationAdvertising(Database.AdsType.EmbedMessageContent, 1).FirstOrDefault();
            advertise.Add(contentAdvertises);

            await FollowupAsync(
                text: Context.User.Mention + " " + contentAdvertises?.Url,
                components: GetComponent());

        }


        private MessageComponent GetComponent()
        {
            var components = new ComponentBuilder();

            GetTopPlayerButton(components, 0);
            GetTeamsButton(components, 0);
            GetProfileButton(components, 1);

            var advertises = adsService.ExploitationAdvertising(Database.AdsType.Buttons, 2);
            foreach (var ad in advertises)
            {
                components.WithButton(ad.Label, null, ButtonStyle.Link, new Emoji("✨"), ad.Url, false, 2);
            }

            components.WithButton(CancelButton.GetButton(userLocale), 2);
            return components.Build();
        }
        private void GetProfileButton(ComponentBuilder cb, int row)
        {
            if (Context.BotUser.Accounts.Any() is false) return;

            var btn = new ButtonBuilder()
            {
                Label = userLocale["menu.dynastio.btn.profile.text"],
                Style = ButtonStyle.Primary,
                Emote = ProfileButton.Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = ProfileButton.CustomId
            };
            cb.WithButton(btn, row);
        }
        private void GetTeamsButton(ComponentBuilder cb, int row)
        {
            var btn = new ButtonBuilder()
            {
                Label = userLocale["menu.dynastio.btn.teams.text", dynastio.OnlinePlayers.GroupBy(a => a.Team).Count()],
                Style = ButtonStyle.Primary,
                Emote = TeamsButton.Emoji,
                IsDisabled = true,
                Url = null,
                CustomId = TeamsButton.CustomId
            };
            cb.WithButton(btn, row);
        }
        private void GetTopPlayerButton(ComponentBuilder cb, int row)
        {
            var btn = new ButtonBuilder()
            {
                Label = userLocale["menu.dynastio.btn.players.text", dynastio.OnlinePlayers.Count],
                Style = ButtonStyle.Primary,
                Emote = PlayersButton.Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = PlayersButton.CustomId
            };
            cb.WithButton(btn, row);
        }

    }
}
