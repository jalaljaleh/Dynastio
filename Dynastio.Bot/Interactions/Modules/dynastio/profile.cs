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

namespace Dynastio.Bot.Interactions.Modules.Dynastio
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class ProfileModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [RateLimit(30, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile()
        {
            await DeferAsync();

            string accountId = Context.BotUser.Accounts.FirstOrDefault().Id;

            var result = await Dynastio.GetUserProfileAsync(accountId);

            var msg = await FollowupAsync(Context.User.Id.ToUserMention() + $"Conis: " + result.Coins);
        }


    }
}
