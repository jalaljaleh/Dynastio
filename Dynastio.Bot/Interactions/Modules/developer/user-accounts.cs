using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Reflection;
using Dynastio.Bot.Interactions.AutoCompeletes;

namespace Dynastio.Bot.Interactions.modules
{
    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [RequireRole(480954902005415937)]
    [Group("developer", "developer")]
    public class developerModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public RankService _rankService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public IDynastioBotDatabase _database { get; set; }

        [Group("user-accounts", "user commands")]
        public class userModule : OwnerModule
        {

            [SlashCommand("list", "dynastio accounts")]
            public async Task list(IGuildUser user)
            {
                await DeferAsync();

                var buser = await _userService.GetUserAsync(user.Id, false);
                if (buser is null)
                {
                    await FollowupAsync("no any result found.");
                    return;
                }

                var message = await FollowupAsync(Context.User.Id.ToUserMention(),
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

                                      ?? this["no_account_found"].ToMarkdown()),

                        Color = Color.Orange,
                        Url = "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1"
                    }.Build());
            }

            [SlashCommand("details", "get a connected account details")]
            public async Task details(IGuildUser user, [Autocomplete(typeof(AutoCompeleteAccounts))] string account)
            {
                await DeferAsync(false);

                var buser = await _userService.GetUserAsync(user.Id, false);
                if (buser is null)
                {
                    await FollowupAsync("no any result found.");
                    return;
                }

                UserAccount selectedAccount = buser.GetAccountByHashCode(account);

                if (selectedAccount is null) await FollowupAsync("account not found.");
                else await FollowupAsync(Context.User.Id.ToUserMention(),
                        embed: (
                        $"\nReminder: `{selectedAccount.Reminder}`" +
                        $"\nAccount Id: `{selectedAccount.Id}`" +
                        $"\nAccount Service: `{selectedAccount.GetAccountService()}`" +
                        $"\nPinCode: `{selectedAccount.PinCode}`" +
                        $"\nAdded at: {selectedAccount.AddedAt.ToDiscordUnixTimestampFormat()}" +
                        $"\nIs Default: {selectedAccount.IsDefault}"
                        ).ToEmbed(user.Username + " Account Details", Color.Green), ephemeral: false);
            }

        }

    }
}
