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
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules.Guild
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireDatabase]
    [Group("accounts", "your dynast.io accounts settings")]
    public class AccountsModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public UserService userManager { get; set; }
        public DynastioClient Dynastio { get; set; }

        [RateLimit(10)]
        [SlashCommand("list", "dynastio accounts")]
        public async Task list()
        {
            await DeferAsync();

            var message = await FollowupAsync(Context.User.Id.ToUserMention(),
                embed: new EmbedBuilder()
                {
                    Title = this["accounts.account.title"],
                    Description = this["accounts.account.description"] + "\n" +
                                  ((Context.BotUser.Accounts?.ToStringTable(new string[] { "#", this["account"] + " |", "Description |", "Default |", "Service |", this["added_at"] },
                                  a => Context.BotUser.Accounts.IndexOf(a) + 1,
                                  a => a.Nickname,
                                  a => a.Description,
                                  a => a.IsDefault ? "Yes" : "No",
                                  a => a.GetAccountService(),
                                  a => a.AddedAt.ToRelative()) + "                 ")
                                  .ToMarkdown() ?? this["no_account_found"].ToMarkdown()),
                    Color = Color.Orange,
                    Url = "https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1"
                }.Build());
        }
        [RateLimit(10)]
        [RequireUserDynastioAccount]
        [SlashCommand("set-default", "set an account as default account")]
        public async Task setDefault([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account)
        {
            await DeferAsync();

            UserAccount selectedAccount = Context.BotUser.GetAccount(int.Parse(account));
            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            Context.BotUser.SwitchDefault(selectedAccount);
            await userManager.UpdateAsync(Context.BotUser);
            await FollowupAsync(Context.User.Id.ToUserMention(), embed: this["account_changed"].ToSuccessfulEmbed(this["account_changed"]));
        }
        [RequireUserDynastioAccount]
        [RateLimit(5)]
        [SlashCommand("edit", "edit your added account")]
        public async Task edit([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account, EditType section, string newValue)
        {
            await DeferAsync();

            UserAccount selectedAccount = Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            switch (section)
            {
                case EditType.Nickname:
                    selectedAccount.Nickname = newValue.TrySubstring(16, false);
                    break;
                case EditType.Reminder:
                    selectedAccount.Reminder = newValue.TrySubstring(150, false);
                    break;
                case EditType.Description:
                    selectedAccount.Description = newValue.TrySubstring(50, false);
                    break;
            }

            Context.BotUser.ReplaceAccount(selectedAccount.Id, selectedAccount);

            await userManager.UpdateAsync(Context.BotUser);

            await FollowupAsync(Context.User.Id.ToUserMention(), embed: this["account_changed"].ToSuccessfulEmbed(this["account_changed"]));
        }
        public enum EditType
        {
            Nickname,
            Reminder,
            Description
        }

        [RequireUserDynastioAccount]
        [RateLimit(3)]
        [RequireConfirmation]
        [SlashCommand("remove", "remove a connected account")]
        public async Task remove([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account)
        {
            await DeferAsync();

            UserAccount selectedAccount = Context.BotUser.GetAccount(int.Parse(account));
            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            Context.BotUser.RemoveAccount(selectedAccount);

            await userManager.UpdateAsync(Context.BotUser);

            await FollowupAsync(Context.User.Id.ToUserMention(), embed: this["account_removed"].ToSuccessfulEmbed("account_removed"));
        }
        [RequireUserDynastioAccount]
        [RateLimit(5, 2)]
        [SlashCommand("details", "get a connected account details")]
        public async Task id([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account)
        {
            await DeferAsync(true);

            UserAccount selectedAccount = Context.BotUser.GetAccount(int.Parse(account));

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }

            await FollowupAsync(Context.User.Id.ToUserMention(),
                embed: (
                $"\nNickname: {selectedAccount.Nickname}" +
                $"\nAccount Id: `{selectedAccount.Id}`" +
                $"\nDescription: `{selectedAccount.Description}`" +
                $"\nAccount Service: `{selectedAccount.GetAccountService()}`" +
                $"\nAdded at: {selectedAccount.AddedAt.ToDiscordUnixTimestampFormat()}" +
                $"\nIs Default: {selectedAccount.IsDefault}" +
                $"\nReminder: ```{selectedAccount.Reminder}```"
                ).ToSuccessfulEmbed(), ephemeral: true);
        }
        [RateLimit(10)]
        [SlashCommand("add", "connect new account to the bot")]
        [RequireConfirmation(
            "Warning",
            "Warning: Make sure the account you are trying to connect is your own, otherwise we reserve the right to block your access to the bot.",
            50, "Its My Own Account", "Its not my account")]
        public async Task addaccount()
        {
            if (Context.BotUser.IsBannedToAddNewAccount)
            {
                await FollowupAsync(embed: this["unauthorized"].ToDangerEmbed(this["unauthorized"]));
                return;
            }

            var modal = new ModalBuilder(this["add_account"], $"accounts add")
               .AddTextInput(new TextInputBuilder(this["nickname"], "nickname", TextInputStyle.Short, this["custom_nickname"], 1, 16, true, null))
               .AddTextInput(new TextInputBuilder(this["coin"], "coin", TextInputStyle.Short, this["account_coins_number"], 1, 16, true, null))
               .AddTextInput(new TextInputBuilder(this["account_id"], "id", TextInputStyle.Short, "google:00000000000000000000", 1, 150, true, null))
               .AddTextInput(new TextInputBuilder("Reminder", "reminder", TextInputStyle.Paragraph, "its a private field you can write anything.", 0, 500, false, null))
               .AddTextInput(new TextInputBuilder("Description", "description", TextInputStyle.Short, "write something about this account.", 0, 50, false, null))
               .Build();

            await Context.OverridenInteraction.RespondWithModalAsync(modal);
        }
        [ModalInteraction("accounts add", true)]
        public async Task add(AddAccountForm form)
        {
            if (Context.BotUser.IsBannedToAddNewAccount)
            {
                await RespondAsync(embed: this["unauthorized"].ToDangerEmbed(this["unauthorized"]));
                return;
            }

            await DeferAsync();

            if (!int.TryParse(form.Coins, out int value))
            {
                await FollowupAsync(Context.UserMention(), embed: this["wrong_coin"].ToDangerEmbed(this["unauthorized"]));
                return;
            }

            int coins = value;
            string id = form.Id.Trim().Remove("id:", "Id:", "ID:", "iD:"); // don't use tolower
            string nickname = form.Nickname.TrySubstring(16).Trim();
            string reminder = form.Reminder?.TrySubstring(500, false) ?? "none";
            string description = form.Description?.TrySubstring(50, false) ?? "none";

            if (id.Contains("discord"))
            {
                if (!id.Contains(Context.User.Id.ToString()))
                {
                    await FollowupAsync(Context.UserMention(), embed: this["can_add_your_account_only"].ToWarnEmbed(this["unauthorized"]));
                    return;
                }
            }
            if (Context.BotUser.Accounts.Count >= 20)
            {
                await FollowupAsync(Context.UserMention(), embed: this["add_account_limits_20_account.description"].ToWarnEmbed(this["add_account_limits_20_account.title"]));
                return;
            }
            if (Context.BotUser.GetAccount(id) != null)
            {
                await FollowupAsync(Context.UserMention(), embed: this["account_added_already.description"].ToWarnEmbed(this["account_added_already.title"]));
                return;
            }

            Profile acc = await Dynastio.Main.GetUserProfileAsync(id).TryGet();
            if (acc is null)
            {
                await FollowupAsync(Context.UserMention(), embed: this["no_account_found"].ToWarnEmbed(this["no_account_found"]));
                return;
            }
            if (coins != acc.Coins)
            {
                await FollowupAsync(Context.UserMention(), embed: this["wrong_coin"].ToDangerEmbed(this["unauthorized"]));
                return;
            }

            var targetUser = await userManager.GetUserByGameAccountIdAsync(id);
            if (targetUser != null)
            {
                await FollowupAsync(Context.UserMention(), embed: $"This account added by {targetUser.Id.ToUserMention()} already.".ToDangerEmbed(this["unauthorized"]));
                return;
            }

            var account = new UserAccount()
            {
                Id = id,
                Nickname = nickname,
                AddedAt = DateTime.UtcNow,
                IsDefault = false,
                Reminder = reminder,
                Description = description
            };
            Context.BotUser.AddAccount(account);

            await userManager.UpdateAsync(Context.BotUser);

            await FollowupAsync(Context.UserMention(), embed: this["account_added"].ToSuccessfulEmbed(this["account_added.title"]));

        }
    }

}
