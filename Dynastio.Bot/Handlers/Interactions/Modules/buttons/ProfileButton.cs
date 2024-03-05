using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Helpers;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers.Interactions.Modules.buttons
{
    public class ProfileButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }
        public AdvertisingService adsService { get; set; }


        public const string CustomId = "btn.dynastio.profile";
        public static Emoji Emoji => new Emoji("✨");

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

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
                userLocale["account_selection_description"] +
                "\n" +
                userLocale["menu_closes", DateTime.UtcNow.AddSeconds(30).UnixTimestampDiscordFormat()],
                ThumbnailUrl = Context.Client.CurrentUser.TryGetAvatarUrl(),
                Color = EmbedsHelper.ColorWaitingResopnse,
            };
            var message = await ModifyCurrentMessageAsync(Context.User.Mention, components: components, embed: embed.Build());

            var result = await DiscordInput.WaitForSelectMenuFromMessageAsync(Context, message, TimeSpan.FromSeconds(30));
            if (result is null || BotUser.GetAccountByHashCode(result.Data.Values.FirstOrDefault(), out UserAccount account) is null)
            {
                await ModifyCurrentMessageAsync(
                    embed: (userLocale["menu_closed_description"] + "\n\n" + adsService.GetInlineEmbedDescription())
                           .ToEmbed(userLocale["menu_closed_title"],
                           Context.Client.CurrentUser.TryGetAvatarUrl()
                           ));
                return;
            }

            var profileCard = await dynastio.GetUserProfileCardAsync(account.Id).TryAsync();
            if (profileCard.isSuccesful is false)
            {
                await ModifyCurrentMessageAsync(
                    embed: (userLocale["data_load_failure"] + "\n\n" + adsService.GetInlineEmbedDescription())
                                                          .ToEmbed(
                                                                   userLocale["proccess_failed_title"],
                                                                   Context.Client.CurrentUser.TryGetAvatarUrl()
                                                                   ));
                return;
            }

            await DeleteCurrentMessageAsync();

            var image = await dynastioGraphic.GetProfileImageAsync(Context.User.TryGetAvatarUrl(), account.Reminder, profileCard.result);

            await DiscordStream.FollowupWithFileAsync(Context, image, $"profile-card-{Context.User.Id}.png", Context.User.Mention,embed: adsService.GetInlineEmbedDescription().ToEmbed(imageUrl: $"attachment://profile-card-{Context.User.Id}.png"));
        }


    }
}
