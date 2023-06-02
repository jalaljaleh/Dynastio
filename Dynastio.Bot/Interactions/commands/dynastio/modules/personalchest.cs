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
        [SlashCommand("personalchest", "your dynastio personal chest")]
        public async Task personalchest([Autocomplete(typeof(SharedAutocompleteHandler.AccountAutocompleteHandler))] string account = "")
        {
            await DeferAsync();

            UserAccount account_ = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetDefaultAccount()
                    : Context.BotUser.GetAccountByHashCode(account);

            var personalchest = await this._dynastio.GetUserPersonalchestAsync(account_.Id);

            var image = await _graphicService.GetPersonalChestAsync(Context.User as IGuildUser,BotUser, account_, personalchest);

            await DiscordStream.FollowupWithFileAsync(Context, image, $"personalchest-{Context.User.Id}.png", $"");
        }


    }
}
