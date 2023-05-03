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
        public GraphicService GraphicService { get; set; }

        [RateLimit(30, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile()
        {
            await DeferAsync();
            var account = Context.BotUser.GetDefaultAccount();
            var profile = await Dynastio.GetUserProfileAsync(account.Id);

             var image = GraphicService.GetProfile(profile);
            await DiscordStream.FollowupWithFileAsync(Context, image, $"profile-{Context.User.Id}.png", $"");
        }


    }
}
