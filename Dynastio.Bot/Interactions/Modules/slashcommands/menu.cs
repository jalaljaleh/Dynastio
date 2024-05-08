using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Buttons.bot;
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

            var result = TryGetComponents(out ComponentBuilder component);

            await FollowupAsync(
                text: GetContent(result),
                embed: GetEmbedBuilder(result),
                components: component.Build());
        }
        public Embed GetEmbedBuilder(bool componentResult)
        {
            if (componentResult is false)
                return new EmbedBuilder()
                {
                    Title = "Prerequisite Required",
                    Description = "A prerequisite is required. After solving the prerequisite, you can use this command.",
                    ThumbnailUrl = BotAvatarUrl,
                }.Build();

            return null;
        }

        public string GetContent(bool componentResult)
        {
            if (componentResult is false)
            {
                return userMention;
            }

            var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.MessageContent, 1).FirstOrDefault();
            return Context.User.Mention + " | " + contentAdvertises?.GetEmbedLink();
        }
        private bool TryGetComponents(out ComponentBuilder component)
        {
            component = new ComponentBuilder();

            if (BotUser.IsAccountConnected is false)
            {
                component.WithButton(ConnectAccountButton.GetButton(userLocale), 0);
                component.WithButton(CancelButton.GetButton(userLocale), 0);

                return false;
            }

            component
                .WithButton(PlayersButton.GetButton(userLocale, dynastio.OnlinePlayers.Count), 0)
                .WithButton(TeamsButton.GetButton(userLocale, dynastio.OnlinePlayers.GroupBy(a => a.Team).Count()), 0)
                .WithButton(PrivateServersButton.GetButton(userLocale, dynastio.OnlineServers.Where(a => a.IsPrivate).Count()), 0)
                .WithButton(PlayersSearchButton.GetButton(userLocale), 0)


                .WithButton(ProfileButton.GetButton(userLocale, BotUser.Accounts.Any()), 1)
                .WithButton(RankButton.GetButton(userLocale), 1)
                .WithButton(SyncRolesButton.GetButton(userLocale, false), 1)


                .WithButton(LeaderboardButton.GetButton(userLocale), 2)

                .WithButton(VersionButton.GetButton(userLocale), 3)
                .WithButton(AddAccountButton.GetButton(userLocale, BotUser.Accounts.Count > 19), 3);

            component.WithButton(CancelButton.GetButton(userLocale), 4);

            component = advertisingService.ExploitationAdvertisingButtons(component, 4);

            return true;
        }

    }
}
