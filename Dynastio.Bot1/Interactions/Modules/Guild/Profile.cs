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
using SixLabors.ImageSharp;
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules.Guild
{
    [RequireUserDynastioAccount]
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles | ChannelPermission.SendMessages | ChannelPermission.EmbedLinks)]
    [Group("profile", "dynastio profile")]
    public class ProfileSlashCommand : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }

        [RateLimit(70, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("picture", "your dynastio profile")]
        public async Task profile([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                ProfileStyle style = ProfileStyle.Default,
                DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dynastioProvider = Dynastio[provider];

            UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                ? Context.BotUser.GetAccount()
                : Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id).TryGet();
            if (profile == null)
            {
                await FollowupAsync("data not found.");
                return;
            }

            var image = GraphicService.GetProfile(profile,style);
            var msg = await FollowupWithFileAsync(image, "profile.jpeg");
        }

        [RateLimit(3)]
        [SlashCommand("rank", "your dynast.io rank")]
        public async Task rank([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
            bool Cache = true,
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dynastioProvider = Dynastio[provider];

            UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                ? Context.BotUser.GetAccount()
                : Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            var rank = await dynastioProvider.GetUserRanAsync(selectedAccount.Id).TryGet();
            if (rank == null)
            {
                await FollowupAsync(embed: this["data.not_found.description"].ToEmbed(this["data.not_found.title"]));
                return;
            }
            IUser user = Context.User;
            using (HttpClient httpClient = new())
            {
                byte[] data = await httpClient.GetByteArrayAsync(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());

                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (var Avatar = SixLabors.ImageSharp.Image.Load(mem))
                    {
                        var img = GraphicService.GetRank(Avatar, rank, user.Username, user.Discriminator);
                        using (var stream = new MemoryStream())
                        {
                            img.SaveAsJpeg(stream);
                            img.Dispose();
                            var msg = await FollowupWithFileAsync(stream, "img.jpeg", Context.BotUser.Id.ToUserMention());

                        }
                    }
                }

            }
        }
        [RateLimit(200, 3)]
        [RequireUserDynastioAccount]
        [SlashCommand("leaderboard", "your place in leaderboard")]
        public async Task leaderboard_me(
            LeaderboardType leaderboard = LeaderboardType.Monthly,
            [Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            var dynastioProvider = Dynastio[provider];

            UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
               ? Context.BotUser.GetAccount()
               : Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            var result = await dynastioProvider.GetUserSurroundingRankAsync(selectedAccount.Id);
            if (result is null)
            {
                await FollowupAsync(embed: "data not found".ToWarnEmbed("Not found"));
                return;
            }
            UserSurroundingRankRow userSurroundingRank = leaderboard switch
            {
                LeaderboardType.Monthly => result.Montly,
                LeaderboardType.Weekly => result.Weekly,
                LeaderboardType.Daily => result.Daily,
                _ => null
            };
            List<UserSurroundingRankRow> usersSurroundingRank = leaderboard switch
            {
                LeaderboardType.Monthly => result.UsersRankMontly,
                LeaderboardType.Weekly => result.UsersRankWeekly,
                LeaderboardType.Daily => result.UsersRankDaily,
                _ => null
            };

            var user = usersSurroundingRank.FirstOrDefault();
            if (user is null)
            {
                await FollowupAsync(embed: "data not found".ToWarnEmbed("Not found"));
                return;
            }

            var firstUser = await dynastioProvider.GetUserRanAsync(user.Id);
            int index = leaderboard switch
            {
                LeaderboardType.Monthly => firstUser.Monthly,
                LeaderboardType.Weekly => firstUser.Weekly,
                LeaderboardType.Daily => firstUser.Daily,
                _ => 0
            };

            string content = $"**Your rank is: {index + 5}**" +
                              usersSurroundingRank.ToStringTable(new[] { this["index"], this["score"], this["time"], this["nickname"] },
                a => $"{(usersSurroundingRank.IndexOf(a) + index).ToRegularCounter()}",
                a => $"{a.Score.Metric()}",
                a => a.CreatedAt.ToRelative(),
                a => $"{a.Nickname.RemoveLines()}").ToMarkdown();

            await FollowupAsync(Context.User.Id.ToUserMention(), embed: content.ToEmbed(this["leaderboard"] + " Me " + this[leaderboard.ToString().ToLower()]));
        }
        [RequireUserDynastioAccount]
        [SlashCommand("stat", "stat")]
        [RateLimit(60, 2, RateLimit.RateLimitType.User)]
        public async Task stat(
           StatType stat, [Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
           DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                ? Context.BotUser.GetAccount()
                : Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            string type = stat == StatType.Craft || stat == StatType.Gather || stat == StatType.Shop ? "item" : "entity";
            string property = stat.ToString().ToLower();


            var dynastioProvider = Dynastio[provider];
            var stat_ = await dynastioProvider.GetUserStatAsync(selectedAccount.Id).TryGet();
            if (stat_ is null)
            {
                await FollowupAsync(Context.User.Id.ToUserMention(), embed: this["data.not_found.description"].ToEmbed(this["data.not_found.title"]));
                return;
            }
            var image = type switch
            {
                "item" => GraphicService.GetStat(property switch
                {
                    "craft" => stat_.Craft,
                    "gather" => stat_.Gather,
                    "shop" => stat_.Shop,
                    _ => null
                }),
                "entity" => GraphicService.GetStat(property switch
                {
                    "build" => stat_.Build,
                    "kill" => stat_.Kill,
                    "death" => stat_.Death,
                    _ => null
                }),
                _ => null
            };

            await FollowupWithFileAsync(image, "stat.jpeg", Context.User.Id.ToUserMention());
        }

        public enum StatType
        {
            Craft,
            Gather,
            Shop,
            Build,
            Kill,
            Death,
        }

        [RateLimit(150, 2)]
        [SlashCommand("details", "your dynastio profile details")]
        public async Task details([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
           DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var dynastioProvider = Dynastio[provider];

            UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                ? Context.BotUser.GetAccount()
                : Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id);
            string content =
                $"Level: {profile.Details.Level}\n" +
                $"Experience: {profile.Details.Experience}\n" +
                $"Required Experience For New Level: {profile.GetRequireExperienceForNewLevel()}\n" +
                $"Latest Server: {profile.LatestServer}\n" +
                $"Latest Activity: {profile.LastActiveAt.ToDiscordUnixTimestampFormat()}\n" +
                $"Coins: {profile.Coins}\n" +
                $"Unlocked Buildings Count: {profile.UnlockedBuildings?.Count ?? 0}\n" +
                $"Unlocked Recipes Count: {profile.UnlockedRecipes?.Count ?? 0}\n" +
                $"Unlocked Skins Count: {profile.UnlockedSkins?.Count ?? 0}\n" +
                $"Badges: {string.Join(", ", profile.Badges ?? new())}\n" +
                $"";
            await FollowupAsync(embed: content.ToEmbed($"Profile {selectedAccount.Nickname}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()));
        }
        [Group("unlocked", "dynastio profile")]
        [RateLimit(150, 4)]
        public class UnlockedModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public DynastioClient Dynastio { get; set; }

            [SlashCommand("skins", "unlocked skins")]
            public async Task skins([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                       DynastioProviderType provider = DynastioProviderType.Main)
            {
                await DeferAsync();
                var dynastioProvider = Dynastio[provider];

                UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetAccount()
                    : Context.BotUser.GetAccount(int.Parse(account));

                if (selectedAccount is null)
                {
                    await FollowupAsync("account not found.");
                    return;
                }

                var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id);
                var content = profile.UnlockedSkins?.ToStringTable(new string[] { "#", "Unlocked Skins" },
                    a => profile.UnlockedSkins.IndexOf(a).ToRegularCounter(),
                    a => a.ToString()) ?? "no any unlocked skin found.";
                await FollowupAsync(embed: content.ToMarkdown().ToEmbed($"Profile {selectedAccount.Nickname}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()));
            }
            [SlashCommand("buildings", "unlocked buildings")]
            public async Task buildings([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                       DynastioProviderType provider = DynastioProviderType.Main)
            {
                await DeferAsync();
                var dynastioProvider = Dynastio[provider];

                UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetAccount()
                    : Context.BotUser.GetAccount(int.Parse(account));

                if (selectedAccount is null)
                {
                    await FollowupAsync("account not found.");
                    return;
                }

                var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id);
                var content = profile.UnlockedBuildings?.ToStringTable(new string[] { "#", "Unlocked Buildings" },
                    a => profile.UnlockedBuildings.IndexOf(a).ToRegularCounter(),
                    a => a.ToString()) ?? "no any unlocked building found.";
                await FollowupAsync(embed: content.ToMarkdown().ToEmbed($"Profile {selectedAccount.Nickname}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()));
            }
            [SlashCommand("recipes", "unlocked recipes")]
            public async Task recipes([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "",
                       DynastioProviderType provider = DynastioProviderType.Main)
            {
                await DeferAsync();
                var dynastioProvider = Dynastio[provider];

                UserAccount selectedAccount = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetAccount()
                    : Context.BotUser.GetAccount(int.Parse(account));

                if (selectedAccount is null)
                {
                    await FollowupAsync("account not found.");
                    return;
                }

                var profile = await dynastioProvider.GetUserProfileAsync(selectedAccount.Id);
                var content = profile.UnlockedRecipes?.ToStringTable(new string[] { "#", "Unlocked Recipes" },
                    a => profile.UnlockedRecipes.IndexOf(a).ToRegularCounter(),
                    a => a.ToString()) ?? "no any unlocked recipes.";
                await FollowupAsync(embed: content.ToMarkdown().ToEmbed($"Profile {selectedAccount.Nickname}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()));
            }
        }

    }
}
