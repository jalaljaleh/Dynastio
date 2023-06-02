using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Shard;
using System.ComponentModel;
using Dynastio.Bot.Data;

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{

    public partial class DynastioModule
    {
        [RequireUserDynastioAccount]
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "")
        {
            await DeferAsync();

            UserAccount account_ = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetDefaultAccount()
                    : Context.BotUser.GetAccountByHashCode(account);

            var _personalchest = await this._dynastio.GetUserPersonalchestAsync(account_.Id)
                .TryAsync();

            var _profile = await this._dynastio.GetUserProfileAsync(account_.Id)
                .TryAsync();

            var image = await _graphicService.GetPersonalChestAsync(Context.User as IGuildUser, BotUser, account_, _profile.result, _personalchest.result);

            await DiscordStream.FollowupWithFileAsync(Context, image, $"profile-{Context.User.Id}.png", $"");
        }


    }
}
