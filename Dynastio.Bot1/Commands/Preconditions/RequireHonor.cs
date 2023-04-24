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
    public class RequireHonor : PreconditionAttribute
    {
        public int? Count
        {
            get;
        }
        public RequireHonor(int Count)
        {
            this.Count = Count;
        }
      
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if ((context as ICustomCommandContext).BotUser.Honor >= Count)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("You need more honor ."));
        }
    }
}
