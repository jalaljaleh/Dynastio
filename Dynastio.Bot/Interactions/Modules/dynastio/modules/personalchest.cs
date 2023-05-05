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

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{

    public partial class DynastioModule
    {
        [RequireUserDynastioAccount]
        [SlashCommand("personalchest", "your dynastio personal chest")]
        public async Task personalchest()
        {
            await DeferAsync();
            var account = Context.BotUser.GetDefaultAccount();
            var personalchest = await this._dynastio.GetUserPersonalchestAsync(account.Id);
            var image = _graphicService.GetPersonalChest(personalchest);
            await DiscordStream.FollowupWithFileAsync(Context, image, $"personalchest-{Context.User.Id}.png", $"");
        }


    }
}
