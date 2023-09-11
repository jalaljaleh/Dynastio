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
using Dynastio.Bot.Services;
using Dynastio.Bot.Interactions.AutoCompeletes;
using Dynastio.Bot.Interactions.Forms;

namespace Dynastio.Bot.Interactions.modules
{

    [RequireGuildOfficial]
    [Group("accounts", "manage your game accounts")]
    public class AccountsModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public DynastioData _dynastioData { get; set; }
        public InternetService _internetService { get; set; }
        public DynastioData _database { get; set; }
        public DynastioClient _dynastio { get; set; }


        [RateLimit(60)]
        [RequireUserDynastioAccount]
        [SlashCommand("roles", "get roles")]
        public async Task roles()
        {
            await DeferAsync();

            var message = await FollowUpToLoading(this["accounts.sync-roles.checking.title"]);

            await Task.Delay(3000);

            var result = await _guildService.SyncUserBadges(Context.BotUser);

            await message.ModifyAsync(a => a.Embed = new Optional<Embed>(new EmbedBuilder()
            {
                Title = this["accounts.syc-roles.successful.title"],
                Description = this["accounts.syc-roles.successful.description"],
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder() {
                        IsInline = true,
                        Name = this["added_roles"],
                        Value = result.addedRoles.Length < 1 ? "` none `": string.Join("\n",result.addedRoles.Select(a=> $"<@&{a}>"))
                    },
                    new EmbedFieldBuilder() {
                        IsInline = true,
                        Name = this["removed_roles"],
                        Value =result.removedRoles.Length < 1 ?"` none `": string.Join("\n",result.removedRoles.Select(a=> $"<@&{a}>"))
                    },
                },
                ThumbnailUrl = "https://cdn.discordapp.com/attachments/1098332386674085988/1106646704662528030/download.png",
                Color = Color.DarkGreen
            }.Build()));

        }

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
        public async Task id([MaxLength(20), Autocomplete(typeof(AutoCompeleteAccounts))] string account)
        {
            await DeferAsync();

            UserAccount selectedAccount = Context.BotUser.GetAccountByHashCode(account);

            Context.BotUser.SwitchAccount(ref selectedAccount);

            await _dynastioData.UpdateAsync(Context.BotUser);

            await FollowupAsync(userMention, embed: this["done"].ToEmbed(this["account_switched"], Color.Green));

        }

        [RateLimit(5, 2)]
        [RequireUserDynastioAccount]
        [SlashCommand("details", "get a connected account details")]
        public async Task details([MaxLength(20), Autocomplete(typeof(AutoCompeleteAccounts))] string account)
        {
            await DeferAsync(true);

            UserAccount selectedAccount = Context.BotUser.GetAccountByHashCode(account);

            if (selectedAccount is null) await FollowupAsync("account not found.");
            else await FollowupAsync(Context.User.Id.ToUserMention(),
                    embed: (
                    $"\nReminder: `{selectedAccount.Reminder}`" +
                    $"\nAccount Id: `{selectedAccount.Id}`" +
                    $"\nAccount Service: `{selectedAccount.GetAccountService()}`" +
                    $"\nPinCode: `{selectedAccount.PinCode}`" +
                    $"\nEmail: `{selectedAccount.Email}`" +
                    $"\nAdded at: {selectedAccount.AddedAt.ToDiscordUnixTimestampFormat()}" +
                    $"\nIs Default: {selectedAccount.IsDefault}"
                    ).ToEmbed(default, Color.Green), ephemeral: true);
        }

        [RateLimit(5)]
        [RequireUserDynastioAccount]
        [SlashCommand("update", "update account details")]
        public async Task update([MaxLength(20), Autocomplete(typeof(AutoCompeleteAccounts))] string account, string newReminder)
        {
            await DeferAsync(true);

            UserAccount selectedAccount = Context.BotUser.GetAccountByHashCode(account);

            if (selectedAccount is null)
            {
                await FollowupAsync("account not found.");
                return;
            }
            
            selectedAccount.SetReminder(newReminder);

            await _dynastioData.UpdateAsync(Context.BotUser);

            await FollowupAsync(userMention, embed: "Account updated.".ToEmbed());
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
               .AddTextInput(new TextInputBuilder(this["reminder"], "reminder", TextInputStyle.Short, "its a reminder field you can write anything.", 0, 16, false, null))
               .AddTextInput(new TextInputBuilder(this["email"], "email", TextInputStyle.Short, "Gmail, Discord, Facebook, Youtube Channel Address.", 0, 100, false, null))
               .Build();

            await Context.OverridedInteraction.RespondWithModalAsync(modal);
        }

        private const int _maxAccounts = 20;

        [RateLimit(10)]
        [ModalInteraction("accounts add", true,RunMode.Sync)]
        public async Task add(FormAddAccount form)
        {
            await DeferAsync();

            string id = form.Id.Trim().Remove("id:", "Id:", "ID:", "iD:"); // don't use tolower

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
                    await FollowupAsync(userMention, embed: this["error.addaccount.unauthorized.pincode"].ToEmbed(this["error.addaccount.unauthorized.title"]));
                    return;
                }

                var targetUser = await _dynastioData.GetUserByAccountIdAsync(id);
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
                    PinCode = form.PinCode.Trim(),
                    Email = form.Email,
                }.SetReminder(form.Reminder);

                Context.BotUser.Accounts.Add(account);
                Context.BotUser.SwitchAccount(ref account);
                await _dynastioData.UpdateAsync(Context.BotUser);

                await FollowupAsync(userMention, embed: this["account_added"].ToEmbed(this["account_added.title"], Color.Green));

                await _guildService.SyncUserBadges(Context.BotUser);
            }
        }

        [RateLimit(10)]
        [SlashCommand("connect", "connect an account to the bot")]
        public async Task coonectaccounts()
        {
            var modal = new ModalBuilder(this["modal.account.add.title"], $"accounts connect")
               .AddTextInput(new TextInputBuilder(this["account_id"], "id", TextInputStyle.Short, "youtube:0000000000000000000", 1, 150, true, null))
               .AddTextInput(new TextInputBuilder(this["pincode"], "pincode", TextInputStyle.Short, this["XXX-XXX-XXX"], 11, 11, true, null))
               .Build();

            await Context.OverridedInteraction.RespondWithModalAsync(modal);
        }

        [RateLimit(15)]
        [ModalInteraction("accounts connect", true,RunMode.Sync)]
        public async Task connect(FormConnectAccount form)
        {
            await DeferAsync();

            if (string.IsNullOrEmpty(Context.BotUser.gameAccountId) is false)
            {
                await FollowupAsync(userMention, embed: this["error.unauthorized.connected-already"].ToEmbed(this["unauthorized"], Color.Red));
                return;
            }

            string id = form.Id.Trim().Remove("id:", "Id:", "ID:", "iD:"); // don't use tolower

            if (id.Contains("discord") && !id.Contains(Context.User.Id.ToString()))
            {
                await FollowupAsync(userMention, embed: this["error.unauthorized.discord"].ToEmbed(this["unauthorized"], Color.Orange));
                return;
            }

            bool authorized = await _dynastio.GetUserPincodeStatusAsync(id, form.PinCode.Trim());
            if (authorized is false)
            {
                await FollowupAsync(userMention, embed: this["error.addaccount.unauthorized.pincode"].ToEmbed(this["error.addaccount.unauthorized.title"]));
                return;
            }

            var targetUser = await _dynastioData.GetUserByConnectedAccountIdAsync(id);
            if (targetUser != null)
            {
                await FollowupAsync(userMention, embed: $"This account has been connected by {targetUser.Id.ToUserMention()} already.".ToEmbed(this["unauthorized", Color.Red]));
                return;
            }

            Context.BotUser.gameAccountId = id;
            await _dynastioData.UpdateAsync(Context.BotUser);

            await FollowupAsync(userMention, embed: "Account connected succesfuly to your profile !".ToEmbed(this["account_connected.title"], Color.Green));

        }

    }

}
