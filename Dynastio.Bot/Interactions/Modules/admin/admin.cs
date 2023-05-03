using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Admin
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RateLimit(60, 2, RateLimit.RateLimitType.User)]
    public class Admin : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        //[SlashCommand("y", "dynast.io version")]
        //public async Task version()
        //{
        //    await DeferAsync();
          
        //}
    }
}
