using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Data;
using System.ComponentModel.DataAnnotations;
using Dynastio.Bot.Interactions.Modules.Shard;
using Newtonsoft.Json;

namespace Dynastio.Bot.Interactions.Modules.Dynastio
{
    [RequireUser(RequireUserAttribute.RequireUserType.BotOwner)]
    [RequireContext(ContextType.Guild)]
    [EnabledInDm(true)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [Group("developer", "bot developer commands")]
    public class DeveloperModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        
        [Group("database", "database")]
        [RequireDatabase]
        public class DatabaseModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public IDatabaseContext db { get; set; }
           
            [RequireConfirmation(
                "Warning: sensitive information",
                "Make sure this is a private channel, This information may be sensitive, Click the confirm button if you want to continue.")]
            [SlashCommand("backup", "backup database")]
            public async Task BackUpDatabase()
            {
                //await DeferAsync();
              
                var users = db.GetAllUsers();
                var guilds = db.GetAllGuilds();

                var dataUsers = JsonConvert.SerializeObject(users);
                await DiscordStream.SendStringAsFile(Context.Channel, dataUsers, "users.json");

                var dataGuilds = JsonConvert.SerializeObject(guilds);
                await DiscordStream.SendStringAsFile(Context.Channel, dataGuilds, "guilds.json");


                var msg = await FollowupAsync(
                    embed: 
                    ($"Total Users: {users.Count}\n" +
                    $"Total Guilds: {guilds.Count}\n" +
                    $"data uploaded succesfully.").ToSuccessfulEmbed("backup " + DateTime.UtcNow));
            }
        }
        [Group("youtube", "users commands")]
        public class YoutubeModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public YoutubeService youtubeService { get; set; }

            [SlashCommand("videos", "send all youtube videos of a channel")]
            public async Task sendAllYoutubeVideos(string channelId = "", YoutubeVideoOrderByType orderby = YoutubeVideoOrderByType.OrderByNewest, int take = 1, int skip = 0)
            {
                await DeferAsync();

                var videos = channelId.IsNullOrEmpty()
                    ? youtubeService.Videos
                    : await youtubeService.GetAllChannelVideos(channelId).TryGet();

                if (videos == null)
                {
                    await FollowupAsync(embed: $"no video found.".ToEmbed());
                    return;
                }

                videos = orderby == YoutubeVideoOrderByType.OrderByOldest
                    ? videos.OrderBy(a => a.Snippet.PublishedAt).ToList()
                    : videos.OrderByDescending(a => a.Snippet.PublishedAt).ToList();

                videos = videos.Skip(skip).Take(take).ToList();

                if (videos == null)
                {
                    await FollowupAsync(embed: $"no video found after the filters.".ToEmbed());
                    return;
                }

                foreach (var v in videos)
                {
                    await FollowupAsync(v.Id.ToYoutubeVideoUrl());
                    await Task.Delay(1000);
                }

                var msg = await FollowupAsync(embed: $"{videos.Count} videos uploaded to the channel. `this message will be delete after 5 seconds.`".ToEmbed());
                await Task.Delay(15000);
                await msg.DeleteAsync();

            }
            public enum YoutubeVideoOrderByType
            {
                OrderByOldest,
                OrderByNewest,
            }
        }
        [Group("players", "players commands")]
        public class playersModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public UserService UserService { get; set; }
            public DynastioClient Dynastio { get; set; }

            [RateLimit(5, 2, RateLimit.RateLimitType.User)]
            [SlashCommand("find", "find a discord user in the game")]
            public async Task Find(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlinePlayersAutocompleteHandler))] string player = "",
             DynastioProviderType provider = DynastioProviderType.Main)
            {
                await DeferAsync(true);

                var player_ = Dynastio[provider].OnlinePlayers.FirstOrDefault(a => a.UniqeId == player);
                if (player_ is null)
                {
                    await FollowupAsync(embed: "player not found".ToWarnEmbed("Not Found !"));
                    return;
                }
                if (!player_.IsAuth)
                {
                    await FollowupAsync(embed: "the player is a guest".ToWarnEmbed("guest !"));
                    return;
                }
                var team = Dynastio[provider].OnlinePlayers.GroupBy(a => a.Team).FirstOrDefault(a => a.Key == player_.Team && !string.IsNullOrEmpty(a.Key));

                var teammates = team is null ? "`none`" : string.Join(", ", team.Select(a => a.Nickname));
                string content =
                    $"**Nickname:** {player_.Nickname.TrySubstring(18)}\n" +
                    $"**Account Id:** {player_.Id ?? ""}\n" +
                    $"**Level:** {player_.Level}\n" +
                    $"**Score:** {player_.Score.Metric()}\n" +
                    $"**Server:** {player_.Parent.Label.TrySubstring(20)}\n" +
                    $"**Team:** {player_.Team}\n" +
                    $"**Teammates:**: {teammates.ToMarkdown()}";

                await FollowupAsync(embed: content.ToEmbed(player_.Nickname + "(Online Player)"));
            }
        }

        [Group("users", "users commands")]
        public class UsersModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            public UserService userManager { get; set; }

            [RequireConfirmation]
            [SlashCommand("clear-cache", "clear cached users")]
            public async Task clearCache()
            {
               // await DeferAsync();
                var usersCount = userManager.GetCacheCount();
                userManager.ClearCache();
                await FollowupAsync(embed: $"{usersCount} users removed from the cache".ToEmbed());
            }
           
            [RequireConfirmation]
            [SlashCommand("accounts-remove", "remove an account from the user")]
            public async Task removeAccount(IUser user, [Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "")
            {
               // await DeferAsync();
                var buser = await userManager.GetUserAsync(user.Id);

                var selectedAccount = buser.GetAccount(int.Parse(account));
                if (selectedAccount is null)
                {
                    await FollowupAsync(embed: $"account not found.".ToEmbed());
                    return;
                }
                buser.RemoveAccount(selectedAccount);
                await userManager.UpdateAsync(buser);
                await FollowupAsync(embed: $"account removed from the user.".ToEmbed());
            }
            [RequireConfirmation]
            [SlashCommand("permissions", "ban a user from the bot")]
            public async Task userPermissiond(IUser user, UserBanType section, PermissionsType status)
            {
                //await DeferAsync();
                var buser = await userManager.GetUserAsync(user.Id);

                switch (section)
                {
                    case UserBanType.Bot:
                        buser.IsBanned = status == PermissionsType.Limited;
                        break;
                    case UserBanType.AddAccount:
                        buser.IsBannedToAddNewAccount = status == PermissionsType.Limited;
                        break;
                    case UserBanType.AddPrivateServerCommand:
                        buser.IsBannedToAddNewPrivateServerCommand = status == PermissionsType.Limited;
                        break;
                }
              
                await userManager.UpdateAsync(buser);

                await FollowupAsync(embed: $"the {user.Id.ToUserMention()} permission to ` {section} ` is ` {status} `.".ToEmbed());
            }
            public enum UserBanType
            {
                Bot,
                [Display(Name = "Add Account")]
                AddAccount,
                [Display(Name = "Add command to the private server commands.")]
                AddPrivateServerCommand
            }
            public enum PermissionsType
            {
                Limited,
                [Display(Name = "Not Limited")]
                NotLimited
            }

        }

    }

}
