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
    public class PersonalChestModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }
        public GraphicService GraphicService { get; set; }

        [RateLimit(30, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("personalchest", "your dynastio personal chest")]
        public async Task personalchest()
        {
            await DeferAsync();
            var account = Context.BotUser.GetDefaultAccount();
            var personalchest = await Dynastio.GetUserPersonalchestAsync(account.Id);
            var image = GraphicService.GetPersonalChest(personalchest);
            await DiscordStream.FollowupWithFileAsync(Context, image, $"personalchest-{Context.User.Id}.png", $"");
        }


    }
}
