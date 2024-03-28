using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Services;
using Dynastio.Bot.Database;
using MongoDB.Bson;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Addons;
using System.Reflection;
using Dynastio.Bot.Interactions.AutoCompeletes;
using Newtonsoft.Json;
using Dynastio.Bot.Handlers;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{
    [Group("developer", "developer")]
    [RequireTeamMemberAttribute]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [IntegrationType(ApplicationIntegrationType.UserInstall)]
    public class DeveloperModule : BotInteractionModuleBase
    {
        [SlashCommand("ping", "ping ")]
        public async Task ping()
        {
            await DeferAsync(true);

            await FollowupAsync(
                embed: ($"Successful Operator" +
                       $"ping: {Context.Client.Latency}\n" +
                       $"start up: {Main.StartUp.UnixTimestampDiscordFormat()}\n" +
                       $"version: {Assembly.GetCallingAssembly().GetHashCode().ToString()}\n" +
                       $"").ToEmbed("Pong !"));
        }


        [Group("guilds", "guild")]
        public class GuildModule : BotInteractionModuleBase
        {

                public EventsHandler eventsHandler { get; set; }

                [SlashCommand("partner-roles-set", "set ")]
                public async Task set(IRole role)
                {
                    await DeferAsync(true);

                    this.BotGuild.PartnersRoleId = role.Id;

                    await UpdateBotGuildAsync();

                    await FollowupAsync(embed: "Done.".ToEmbed("Successful Operator"));
                }

                [SlashCommand("roles-sync", "sync ")]
                public async Task sync()
                {
                    await DeferAsync(true);

                    await eventsHandler._ready_event.SyncSub();

                    await FollowupAsync(embed: "Done .".ToEmbed("Successful Operator"));
                }
            
        }


        [Group("advertisement", "advertisement")]
        public class AdvertisementModule : BotInteractionModuleBase
        {
            public AdvertisingService advertisingService { get; set; }

            [SlashCommand("delete", "delete ads")]
            public async Task delete(string Id)
            {
                await DeferAsync(true);

                var target = advertisingService.GetRemainingAdvertises().FirstOrDefault(a => a.Id.ToString() == Id);

                var ads = advertisingService.DeleteAdvertise(target);

                await FollowupAsync(embed: "Done, the record deleted from the database.".ToEmbed("Successful Operator"));
            }
            [SlashCommand("current", "current ads")]
            public async Task current(IUser user = null)
            {
                await DeferAsync(true);

                var ads = advertisingService.GetRemainingAdvertises();

                if (user is not null)
                    ads = ads.Where(a => a.User == user.Id).ToList();

                var content = ads.ToStringTable(new string[] { "Id", "Count", "Type", "StartedAt", "UserId" },

                    a => a.Id.ToString(),
                    a => a.DisplayCount + "/" + a.Count,
                    a => (int)a.Type,
                    a => a.StartedAt.ToString("d"),
                    a => a.User);

                await FollowupAsync(
                    text: Context.User.Mention + "\n" + content.ToMarkdown());
            }

            [SlashCommand("insert", "insert new ads")]
            public async Task insert(string label, string url, AdsType type, int count, IUser user, string btnEmoji = null)
            {
                await DeferAsync(true);

                if (btnEmoji is not null)
                {
                    try
                    {
                        var emote = new Emoji(btnEmoji);
                    }
                    catch
                    {
                        await FollowupAsync("emoji not valid");
                        return;
                    }
                }

                var ad = new Advertise()
                {
                    Id = ObjectId.GenerateNewId(),
                    Label = label,
                    Url = url,
                    Count = count,
                    Type = type,
                    DisplayCount = 0,
                    StartedAt = DateTime.UtcNow,
                    User = user.Id,
                    FinishedAt = DateTime.MinValue,
                    Emoji = btnEmoji
                };
                await advertisingService.InsertAndCache(ad);

                await FollowupAsync(
                    text: Context.User.Mention,
                    embed: new EmbedBuilder()
                    {
                        Title = "Data inserted",
                        Description =
                        (
                        $"Id: {ad.Id}\n" +
                        $"Label:  {label} \n" +
                        $"Url:  {url} \n" +
                        $"type:  {type} \n" +
                        $"count:  {count} \n" +
                        $"customer:  <@{user.Id}> \n" +
                        $"").ToMarkdown(),
                        ThumbnailUrl =
                        Context.Client.CurrentUser.GetAvatarUrl() ??
                        Context.Client.CurrentUser.GetDefaultAvatarUrl() ??
                        Context.Guild.IconUrl,
                    }.Build());
            }
        }


        [Group("cache", "cache")]
        public class CacheModule : BotInteractionModuleBase
        {

            [SlashCommand("clear", "cache ")]
            public async Task clear()
            {
                await DeferAsync(true);

                this.Context._dynastioData.ClearCache();

                await FollowupAsync(embed: "Done, user and guilds cleared.".ToEmbed("Successful Operator"));
            }


        }
        [Group("user-accounts", "user commands")]
        public class userModule : BotInteractionModuleBase
        {
            [SlashCommand("download-json", "dynastio accounts")]
            public async Task json(IGuildUser user)
            {
                await DeferAsync();

                var buser = await dynastioBotDatabase.GetUserAsync(user.Id, false);
                if (buser is null)
                {
                    await FollowupAsync("no any result found.");
                    return;
                }

                await DiscordStream.SendStringAsFile(Context.Channel, JsonConvert.SerializeObject(buser));
                await FollowupAsync("result:");
            }
            [SlashCommand("list", "dynastio accounts")]
            public async Task list(IGuildUser user)
            {
                await DeferAsync();

                var buser = await dynastioBotDatabase.GetUserAsync(user.Id, false);
                if (buser is null)
                {
                    await FollowupAsync("no any result found.");
                    return;
                }

                var message = await FollowupAsync(Context.User.Mention,
                    embed: new EmbedBuilder()
                    {
                        Title = this["accounts.account.title"],
                        Description = this["accounts.account.list.description"] + "\n" +
                                      ((buser.Accounts?.ToStringTable(new string[] { "#", this["account"] + " |", "Default |", "Service |", this["added_at"] },
                                      a => buser.Accounts.IndexOf(a) + 1,
                                      a => a.Reminder,
                                      a => a.IsDefault ? "Yes" : "No",
                                      a => a.GetAccountService(),
                                      a => a.AddedAt.ToRelative()) + "                 ").ToMarkdown()

                                      ?? this["no_account_found"].ToMarkdown()) +
                                      $"Main Account: {buser.gameAccountId}".ToMarkdown(),

                        Color = Color.Orange,
                        Url = "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1"
                    }.Build());
            }

            [SlashCommand("details", "get a connected account details")]
            public async Task details(IGuildUser user, [Autocomplete(typeof(AutoCompeleteAccounts))] string account)
            {
                await DeferAsync(false);

                var buser = await dynastioBotDatabase.GetUserAsync(user.Id, false);
                if (buser is null)
                {
                    await FollowupAsync("no any result found.");
                    return;
                }

                buser.GetAccountByHashCode(account, out UserAccount selectedAccount);

                if (selectedAccount is null) await FollowupAsync("account not found.");
                else await FollowupAsync(Context.User.Mention,
                        embed: (
                        $"User: {user.Mention}" +
                        $"\nMain Account: ` {buser.gameAccountId} `" +
                        $"\nAccounts Count: ` {buser.Accounts.Count} `" +
                        $"\nReminder: `{selectedAccount.Reminder}`" +
                        $"\nAccount Id: `{selectedAccount.Id}`" +
                        $"\nAccount Service: `{selectedAccount.GetAccountService()}`" +
                        $"\nPinCode: `{selectedAccount.PinCode}`" +
                        $"\nAdded at: {selectedAccount.AddedAt.UnixTimestampDiscordFormat()}" +
                        $"\nIs Default: {selectedAccount.IsDefault}" +
                        $"\nEmail: {selectedAccount.Email}"
                        ).ToEmbed(user.Username + " Account Details", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(), color: Color.Green), ephemeral: false);
            }
        }
    }

}
