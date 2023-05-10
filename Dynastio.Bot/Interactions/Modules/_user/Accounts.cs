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
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules._user
{

    [Group("accounts", "manage your game accounts")]
    public class AccountsModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public IDynastioBotDatabase _database { get; set; }
        public DynastioClient _dynastio { get; set; }

        [RateLimit(10)]
        [RequireUserDynastioAccount]
        [SlashCommand("list", "dynastio accounts")]
        public async Task list()
        {
            await DeferAsync();

            var message = await FollowupAsync(Context.User.Id.ToUserMention(),
                embed: new EmbedBuilder()
                {
                    Title = this["accounts.account.title"],
                    Description = this["accounts.account.list.description"] + "\n" +
                                  ((Context.BotUser.Accounts?.ToStringTable(new string[] { "#", this["account"] + " |", "Default |", "Service |", this["added_at"] },
                                  a => Context.BotUser.Accounts.IndexOf(a) + 1,
                                  a => a.Reminder,
                                  a => a.IsDefault ? "Yes" : "No",
                                  a => a.GetAccountService(),
                                  a => a.AddedAt.ToRelative()) + "                 ").ToMarkdown()

                                  ?? this["no_account_found"].ToMarkdown()),

                    Color = Color.Orange,
                    Url = "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1"
                }.Build());
        }
        [RateLimit(5)]
        [RequireUserDynastioAccount]
        [SlashCommand("switch", "switch to another account")]
        public async Task id([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account)
        {
            await DeferAsync(true);

            UserAccount selectedAccount = Context.BotUser.Accounts
                .FirstOrDefault(
                a =>
                a.GetHashCode().Equals(int.Parse(account)));

            Context.BotUser.SwitchAccount(selectedAccount);

            await _userService.UpdateAsync(Context.BotUser);

            await FollowupAsync(userMention, embed: this["done"].ToEmbed(this["account_switched"], Color.Green));

        }
        [RateLimit(5, 2)]
        [RequireUserDynastioAccount]
        [SlashCommand("details", "get a connected account details")]
        public async Task details([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account)
        {
            await DeferAsync(true);

            UserAccount selectedAccount = Context.BotUser.Accounts
                .FirstOrDefault(
                a =>
                a.GetHashCode().Equals(int.Parse(account)));

            if (selectedAccount is null) await FollowupAsync("account not found.");
            else await FollowupAsync(Context.User.Id.ToUserMention(),
                    embed: (
                    $"\nReminder: `{selectedAccount.Reminder}`" +
                    $"\nAccount Id: `{selectedAccount.Id}`" +
                    $"\nAccount Service: `{selectedAccount.GetAccountService()}`" +
                    $"\nPinCode: `{selectedAccount.PinCode}`" +
                    $"\nAdded at: {selectedAccount.AddedAt.ToDiscordUnixTimestampFormat()}" +
                    $"\nIs Default: {selectedAccount.IsDefault}"
                    ).ToEmbed(default, Color.Green), ephemeral: true);
        }

        [RateLimit(10)]
        [SlashCommand("add", "connect an account to the bot")]
        [RequireConfirmation(
            "Warning",
            "Warning: Make sure the account you are trying to connect is your own, otherwise we reserve the right to block your access to the bot.",
            50,
            "Agree",
            "Its not my account")]
        public async Task addaccount()
        {
            var modal = new ModalBuilder(this["modal.account.add.title"], $"accounts add")
               .AddTextInput(new TextInputBuilder(this["account_id"], "id", TextInputStyle.Short, "google:0000000000000000000", 1, 150, true, null))
               .AddTextInput(new TextInputBuilder(this["pincode"], "pincode", TextInputStyle.Short, this["XXX-XXX-XXX"], 11, 11, true, null))
               .AddTextInput(new TextInputBuilder(this["reminder"], "reminder", TextInputStyle.Paragraph, "its a reminder field you can write anything.", 0, 500, false, null))
               .Build();

            await Context.OverridedInteraction.RespondWithModalAsync(modal);
        }

        private const int _maxAccounts = 4;

        [RateLimit(10)]
        [ModalInteraction("accounts add", true)]
        public async Task add(forms.AddAccountForm form)
        {
            await DeferAsync();

            string id = form.Id.Trim().Remove("id:", "Id:", "ID:", "iD:"); // don't use tolower
            string reminder = string.IsNullOrEmpty(form.Reminder)
                ? id.Split(":")[0]
                : form.Reminder.TrySubstring(16).Trim();

            if (id.Contains("discord") && !id.Contains(Context.User.Id.ToString()))
                await FollowupAsync(userMention, embed: this["error.unauthorized.discord"].ToEmbed(this["unauthorized"], Color.Orange));
            else if (Context.BotUser.Accounts.Count >= _maxAccounts)
                await FollowupAsync(userMention, embed: this["error.addaccount.limit.description:[x]", _maxAccounts].ToEmbed(this["error.addaccount.limit.title"]));
            else if (Context.BotUser.GetAccount(id) != null)
                await FollowupAsync(userMention, embed: this["error.addaccount.duplicated.description"].ToEmbed(this["error.addaccount.duplicated.title"]));
            else
            {
                bool authorized = await _dynastio.GetUserPincodeStatusAsync(id, form.PinCode.Trim());
                if (authorized is false)
                {
                    await FollowupAsync(userMention, embed: this["error.addaccount.unauthorized"].ToEmbed(this["error.addaccount.unauthorized.title"]));
                    return;
                }

                var targetUser = await _userService.GetUserByAccountIdAsync(id);
                if (targetUser != null)
                {
                    await FollowupAsync(userMention, embed: $"This account has been added by {targetUser.Id.ToUserMention()} already.".ToEmbed(this["unauthorized", Color.Red]));
                    return;
                }

                var account = new UserAccount()
                {
                    Id = id,
                    AddedAt = DateTime.UtcNow,
                    IsDefault = false,
                    Reminder = reminder,
                    PinCode = form.PinCode.Trim()
                };
                Context.BotUser.Accounts.Add(account);
                Context.BotUser.SwitchAccount(account);
                await _userService.UpdateAsync(Context.BotUser);

                await FollowupAsync(userMention, embed: this["account_added"].ToEmbed(this["account_added.title"], Color.Green));
            }
        }
    }

}
