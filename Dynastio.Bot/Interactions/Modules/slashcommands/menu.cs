using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    [RequireBotPermission(ChannelPermission.AttachFiles)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class MenuModule : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }

        [SlashCommand("menu", "dynastio menu")]
        [RateLimit(6, 1)]
        public async Task menu()
        {
            await DeferAsync();

            var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.EmbedMessageContent, 1).FirstOrDefault();

            await FollowupAsync(
                text: Context.User.Mention + " " + contentAdvertises?.GetEmbedLink(),
                components: GetComponent());
        }


        private MessageComponent GetComponent()
        {
            var cBuilder = new ComponentBuilder()
                .WithButton(PlayersButton.GetButton(userLocale, dynastio.OnlinePlayers.Count), 0)
                .WithButton(TeamsButton.GetButton(userLocale, dynastio.OnlinePlayers.GroupBy(a => a.Team).Count()), 0)
                .WithButton(PrivateServersButton.GetButton(userLocale, dynastio.OnlineServers.Where(a => a.IsPrivate).Count()), 0)
                .WithButton(PlayersSearchButton.GetButton(userLocale), 0)

                .WithButton(ProfileButton.GetButton(userLocale, BotUser.Accounts.Any()), 1)
                .WithButton(RankButton.GetButton(userLocale), 1)


                .WithButton(LeaderboardButton.GetButton(userLocale), 2)

                .WithButton(VersionButton.GetButton(userLocale), 3)
                .WithButton(AddAccountButton.GetButton(userLocale, BotUser.Accounts.Count > 19), 3);

            if (BotUser.IsAccountConnected is false)
                cBuilder.WithButton(ConnectAccountButton.GetButton(userLocale));


            cBuilder.WithButton(CancelButton.GetButton(userLocale), 4);


            var advertises = advertisingService.ExploitationAdvertising(Database.AdsType.Buttons, 4);
            foreach (var ad in advertises)
                cBuilder.WithButton(
                    ad.Label,
                    null,
                    ButtonStyle.Link,
                    string.IsNullOrEmpty(ad.Emoji) ? null : new Emoji(ad.Emoji),
                    ad.Url,
                    false,
                    4);


            return cBuilder.Build();
        }

    }
}
