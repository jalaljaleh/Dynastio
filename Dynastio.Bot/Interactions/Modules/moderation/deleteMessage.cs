using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Moderation
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RateLimit(60, 2, RateLimit.RateLimitType.User)]
    public class deleteMessage : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        //[MessageCommand("delete message")]
        //public async Task deletemessage(IMessage message)
        //{
        //    await DeferAsync(true);
        //    if (true)
        //    {

        //    }
        //    await message.DeleteAsync();

        //}
        //[ComponentInteraction("delete-message-request:*:*")]
        //public async Task deletemessagerequest(string channelId, string messageId)
        //{
        //    await DeferAsync(true);
        //}
    }
}
