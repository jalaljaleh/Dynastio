using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Enums;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Extenstions;
using Dynastio.Bot.Database;
using Dynastio.Bot.Helpers;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Modules.buttons;
using System.Threading;

namespace Dynastio.Bot.Interactions.Modules.Buttons.bot
{
    public class ConnectAccountButton : BotInteractionModuleBase
    {
        public const string CustomId = "btn.dynastio.connectaccount";
        public static Emoji Emoji => new Emoji("➕");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.dynastio.connectaccount.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }

        public InteractionService InteractionService { get; set; }
        public XpRankingSystemService rankingService { get; set; }
        public IServiceProvider Service { get; set; }


        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            if (BotUser.Accounts.Count == 0)
            {
                await InteractionService.ComponentCommands
                     .FirstOrDefault(a => a.Name == AddAccountButton.CustomId)
                     .ExecuteAsync(Context, services);

                return;
            }

            await DeferAsync();

            if (BotUser.IsAccountConnected)
            {
                await ModifyCurrentMessageAsync(Context.User.Mention, embed: "the game account connected already".ToEmbed("connected already"));
                return;
            }

            var confirm = await ConfirmActionAsync();
            if (confirm is false)
            {
                await CloseMenuAsync();
                return;
            }

            var account = await SelectUserAccountAsync();
            if (account is null)
            {
                await CloseMenuAsync();
                return;
            }

            var check = await dynastioBotDatabase.GetUserByConnectedAccountIdAsync(account.Id);
            if (check != null)
            {
                BotUser.Accounts.Remove(account);
                await ModifyCurrentMessageAsync(userMention + "@developer, @admin, this is a not a normal action !");

            }
            else
            {
                BotUser.gameAccountId = account.Id;
                await ModifyCurrentMessageAsync(userMention + " Done, Your Main account connected to the bot.");

            }

            await dynastioBotDatabase.UpdateAsync(BotUser);

        }

    }
}
