using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Dynastio.Bot;
using Dynastio.Bot.Commands;

namespace Discord.Commands
{

    public class RequireHonorTime : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext _context, CommandInfo command, IServiceProvider services)
        {
            var context = (ICustomCommandContext)_context;
            var t = (context.BotUser.LastHonorGift - DateTime.UtcNow);
            var time = (t.TotalMinutes);
            if (time <= 0)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? (DateTime.UtcNow + t).ToDiscordUnixTimestampFormat()));
        }

    }
}
