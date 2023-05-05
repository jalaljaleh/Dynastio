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
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile()
        {
            await DeferAsync();
            var account = Context.BotUser.GetDefaultAccount();
            var profile = await _dynastio.GetUserProfileAsync(account.Id);

            var image = _graphicService.GetProfile(profile);
            await DiscordStream.FollowupWithFileAsync(Context, image, $"profile-{Context.User.Id}.png", $"");
        }
    }
}
