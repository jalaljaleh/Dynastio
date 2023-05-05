using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RateLimit(10, 4, RateLimit.RateLimitType.User)]
    public partial class DynastioModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient _dynastio { get; set; }
        public GraphicService _graphicService { get; set; }
    }
}
