using Discord;
using Discord.Interactions;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions.Forms;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Modals.Forms;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Extenstions;

namespace Dynastio.Bot.Interactions.Modules.Modals
{
    public class AddAccountModal : BotInteractionModuleBase
    {
        public const string CustomId = "modal.dynastio.accounts.add";
        public const int MaxAccounts = 20;

        public DynastioApi dynastio { get; set; }

        [ModalInteraction(CustomId)]
        public async Task Execute(AddAccountModalForm form)
        {
            await DeferAsync();
            string id = form.Id.Remove("id:", "Id:", "ID:", "iD:").Trim(); // don't use tolower

            // check if its his own discord user account
            if (id.Contains("discord") && !id.Contains(Context.User.Id.ToString()))
            {
                await ModifyCurrentMessageAsync(userMention, embed: this["modal.dynastio.accounts.add.unauthorized"].ToEmbed(this["unauthorized"], Color.Orange));
                return;
            }

            if (Context.BotUser.Accounts.Count >= MaxAccounts)
            {
                await ModifyCurrentMessageAsync(userMention, embed: this["modal.dynastio.accounts.add.unauthorized.maxaccountlimit.description"].ToEmbed(this["modal.dynastio.accounts.add.unauthorized.maxaccountlimit.description.title"]));
                return;
            }

            if (Context.BotUser.GetAccount(id) != null)
            {
                await ModifyCurrentMessageAsync(userMention, embed: this["modal.dynastio.accounts.add.unauthorized.duplicated.description"].ToEmbed(this["modal.dynastio.accounts.add.unauthorized.duplicated.title"]));
                return;
            }


            bool authorized = await dynastio.GetUserPincodeStatusAsync(id, form.PinCode.Trim());
            if (authorized is false)
            {
                await FollowupAsync(userMention, embed: this["modal.dynastio.accounts.add.unauthorized.pincode.description"].ToEmbed(this["modal.dynastio.accounts.add.unauthorized.pincode.title"]));
                return;
            }

            var targetUser = await this.dynastioBotDatabase.GetUserByAccountIdAsync(id);
            var targetMainAccount = await this.dynastioBotDatabase.GetUserByConnectedAccountIdAsync(id);
            if (targetUser != null && targetMainAccount != null)
            {
                if (targetMainAccount.Id != Context.User.Id)
                {
                    await FollowupAsync(userMention, embed: this["modal.dynastio.accounts.add.unauthorized.used.description", $"<@{targetUser.Id}>"].ToEmbed(this["unauthorized", Color.Red]));
                    return;
                }
            }

            var account = new UserGameAccount()
            {
                Id = id,
                AddedAt = DateTime.UtcNow,
                IsDefault = false,
                PinCode = form.PinCode.Trim(),
                Email = form.Email,
            }.SetReminder(form.Reminder);

            Context.BotUser.Accounts.Add(account);
            Context.BotUser.SwitchAccount(ref account);

            await dynastioBotDatabase.UpdateAsync(Context.BotUser);

            await ModifyCurrentMessageAsync(userMention, embed: this["modal.dynastio.accounts.add.succeeful.description"].ToEmbed(this["modal.dynastio.accounts.add.succeeful.title"], Color.Green));

            // await _guildService.SyncUserBadges(Context.BotUser);
        }
    }
}
