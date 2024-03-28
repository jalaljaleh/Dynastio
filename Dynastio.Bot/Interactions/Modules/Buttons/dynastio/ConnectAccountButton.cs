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

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
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
        public RankingService rankingService { get; set; }
        public IServiceProvider Service { get; set; }


        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            if (BotUser.Accounts.Count == 0)
            {
                await InteractionService.ComponentCommands
                     .FirstOrDefault(a => a.Name == AddAccountButton.CustomId)
                     .ExecuteAsync(this.Context, services);

                return;
            }
            await DeferAsync();

            if (BotUser.IsAccountConnected)
            {
                await ModifyCurrentMessageAsync(Context.User.Mention, embed: "the game account connected already".ToEmbed("connected already"));
                return;
            }

            // Combine Defer and SelectMenu creation for efficiency
            var selectMenu = new SelectMenuBuilder(
                DiscordInput.GenerateCustomId("user.accounts"),
                null,
                Context.UserLocale["menu.profile.accounts.choose"],
                1,
                1,
                false,
                ComponentType.SelectMenu
            );

            // Use LINQ to efficiently filter and map accounts
            var options = BotUser.Accounts
                .Take(20)
                .Select(acc => new SelectMenuOptionBuilder(
                    acc.Reminder,
                    acc.GetHashCode().ToString(),
                    acc.GetAccountService(),
                    null,
                    false
                ))
                .ToList();

            selectMenu.WithOptions(options);

            var components = new ComponentBuilder()
                .WithSelectMenu(selectMenu)
                .WithButton(CancelButton.GetButton(userLocale), 1)
                .Build();

            var embed = new EmbedBuilder()
            {
                Title = userLocale["account_selection"],
                Description =
                userLocale["account_selection_description"] + "\n**"+
                userLocale["undo_action_description"] + "**\n" +
                userLocale["menu_closes", DateTime.UtcNow.AddSeconds(30).UnixTimestampDiscordFormat()],
                ThumbnailUrl = Context.Client.CurrentUser.TryGetAvatarUrl(),
                Color = EmbedsHelper.ColorWaitingResopnse,
            };
            var message = await ModifyCurrentMessageAsync(Context.User.Mention, components: components, embed: embed.Build());

            var result = await Context.WaitForSelectMenuFromMessageAsync(message, TimeSpan.FromSeconds(30));
            if (result is null || BotUser.GetAccountByHashCode(result.Data.Values.FirstOrDefault(), out UserAccount account) is null)
            {
                await ModifyCurrentMessageAsync(
                    embed: (userLocale["menu_closed_description"] + "\n\n" + advertisingService.GetInlineEmbedDescription())
                           .ToEmbed(userLocale["menu_closed_title"],
                           Context.Client.CurrentUser.TryGetAvatarUrl()
                           ));
                return;
            }

            BotUser.gameAccountId = account.Id;
            await this.dynastioBotDatabase.UpdateAsync(BotUser);

            var result1 = await rankingService.SynchronizeGameUser(BotGuild, BotUser);


            await ModifyCurrentMessageAsync(userMention + " done !");
        }

    }
}
