using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Data;
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AttachFiles | ChannelPermission.EmbedLinks)]
    [Group("user", "user")]
    public class UserModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }
        public UserService UserService { get; set; }

        [RateLimit(10, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("find", "find a discord user in the game")]
        public async Task Find(IUser user,
                    DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            var botUser = await UserService.GetUserAsync(user.Id, false);
            if (botUser is null || botUser.Accounts == null || botUser.Accounts.Count == 0)
            {
                await FollowupAsync(embed: "No any account of this user found.".ToWarnEmbed("Not Found !", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
                return;
            }

            var accounts = botUser.Accounts.Select(a => a.Id).ToList();

            var player = Dynastio[provider].OnlinePlayers.FirstOrDefault(a => accounts.Contains(a.Id));
            if (player == null)
            {
                await FollowupAsync(embed: "No any online account of this user found.".ToWarnEmbed($"{user.Username} is offline !", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
                return;
            }

            var team = Dynastio[provider].OnlinePlayers.GroupBy(a => a.Team).FirstOrDefault(a => a.Key == player.Team && !string.IsNullOrEmpty(a.Key));

            var teammates = team is null ? "`none`" : string.Join(", ", team.Select(a => a.Nickname));
            string content =
                $"**Nickname:** {player.Nickname.TrySubstring(18)}\n" +
                $"**Level:** {player.Level}\n" +
                $"**Score:** {player.Score.Metric()}\n" +
                $"**Server:** {player.Parent.Label.RemoveHtmlTags().TrySubstring(20)}\n" +
                $"**Team:** {player.Team}\n" +
                $"**Teammates:**: {teammates.ToMarkdown()}";

            var map = GraphicService.GetMap(new List<Player>() { player });

            await FollowupWithFileAsync(map, "map.jpeg", embed: content.ToEmbed(user.Username + " (Online Player)", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
        }

        [RateLimit(70, 4, RateLimit.RateLimitType.User)]
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile(
            IGuildUser user,
            [Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            var dynastioProvider = Dynastio[provider];

            UserAccount selectedAccount;

            var botUser = await UserService.GetUserAsync(user.Id);
            selectedAccount = string.IsNullOrWhiteSpace(account)
            ? botUser.GetAccount()
            : botUser.GetAccount(int.Parse(account));


            var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id).TryGet();
            if (profile == null)
            {
                await FollowupAsync("data not found.");
                return;
            }

            // for making it harder to add the account of this user
            profile.Coins += Program.Random.Next(-50, 50);
            if (profile.Coins < 0)
                profile.Coins = 0;

            var image = GraphicService.GetProfile(profile);

            await FollowupWithFileAsync(image, "profile.jpeg", $"Discord User Profile {user.Nickname} `{user.Username}` Account: `{selectedAccount.Nickname}`");
        }
        [RateLimit(70, 3, RateLimit.RateLimitType.User)]
        [SlashCommand("chest", "your dynastio chest")]
        public async Task chest(
            IGuildUser user,
            bool All = false,
            [Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();



            var dynastioProvider = Dynastio[provider];

            if (All)
            {
                var botUser = await UserService.GetUserAsync(user.Id, false);
                if (botUser == null)
                {
                    await FollowupAsync("user not found.");
                    return;
                }
                var chests = await botUser.Accounts.GetPersonalchests(dynastioProvider);
                if (chests == null || chests.Count < 1)
                {
                    await FollowupAsync("chest not found.");
                    return;
                }
                var image = GraphicService.GetPersonalChests(chests.ToArray());
                await FollowupWithFileAsync(image, "chest.jpeg", $"Discord User Profile {user.Nickname} `{user.Username}` Account: All Accounts");
            }
            else
            {
                UserAccount selectedAccount;

                var botUser = await UserService.GetUserAsync(user.Id);

                selectedAccount = string.IsNullOrWhiteSpace(account)
                ? botUser.GetAccount()
                : botUser.GetAccount(int.Parse(account));


                if (selectedAccount == null)
                {
                    await FollowupAsync("user not found.");
                    return;
                }

                var chest = await dynastioProvider.GetUserPersonalchestAsync(selectedAccount.Id).TryGet();
                if (chest == null)
                {
                    await FollowupAsync("chest not found, join the game and put something to your chest.");
                    return;
                }
                var image = GraphicService.GetPersonalChest(chest);

                await FollowupWithFileAsync(image, "chest.jpeg", $"Discord User Profile {user.Nickname} `{user.Username}` Account: `{selectedAccount.Nickname}`");
            }

        }
    }
}
