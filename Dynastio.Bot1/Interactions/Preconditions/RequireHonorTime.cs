using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot;
using Discord.WebSocket;
using Discord;
using Dynastio.Bot.Interactions;

namespace Discord.Interactions
{

    public class RequireHonorTime : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext _context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var context = (ICustomInteractionContext)_context;
            var t = (context.BotUser.LastHonorGift - DateTime.UtcNow);
            var time = (t.TotalMinutes);
            if (time <= 0)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? (DateTime.UtcNow + t).ToDiscordUnixTimestampFormat()));
        }
    }
}
