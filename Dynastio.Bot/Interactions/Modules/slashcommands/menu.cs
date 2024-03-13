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
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    public class MenuModule : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }


        [SlashCommand("menu", "dynastio menu")]
        [RateLimit(6, 1)]
        public async Task menu()
        {
            await DeferAsync();

            List<Database.Advertise> advertise = new();

            var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.EmbedMessageContent, 1).FirstOrDefault();
            advertise.Add(contentAdvertises);

            await FollowupAsync(
                text: Context.User.Mention + " " + contentAdvertises?.GetEmbedLink(),
                components: GetComponent());

        }


        private MessageComponent GetComponent()
        {
            var cBuilder = new ComponentBuilder()
                .WithButton(PlayersButton.GetButton(userLocale, dynastio.OnlinePlayers.Count), 0)
                .WithButton(TeamsButton.GetButton(userLocale, dynastio.OnlinePlayers.GroupBy(a => a.Team).Count()), 0)
                .WithButton(PlayersSearchButton.GetButton(userLocale), 0)

                .WithButton(ProfileButton.GetButton(userLocale, BotUser.Accounts.Any()), 1)
                .WithButton(RankButton.GetButton(userLocale), 1)


                .WithButton(LeaderboardButton.GetButton(userLocale),2)
                .WithButton(AddAccountButton.GetButton(userLocale, BotUser.Accounts.Count > 19),2);

            var advertises = advertisingService.ExploitationAdvertising(Database.AdsType.Buttons, 4);
            foreach (var ad in advertises)
            {
                cBuilder.WithButton(ad.Label, null, ButtonStyle.Link, string.IsNullOrEmpty(ad.Emoji) ? new Emoji(ad.Emoji) : null, ad.Url, false, 2);
            }

            cBuilder.WithButton(CancelButton.GetButton(userLocale), 4);
            return cBuilder.Build();
        }

    }
}
