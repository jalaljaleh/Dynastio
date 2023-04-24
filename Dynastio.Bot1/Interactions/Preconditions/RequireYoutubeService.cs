using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot;
using Discord.WebSocket;
using Discord;
using Dynastio;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions
{
    public class RequireYoutubeService : PreconditionAttribute
    {
        public new string ErrorMessage = $"Youtube Services is not ready, ask bot developers to fix the service.";
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {

            if (Program.IsYoutubeServiceInitialized)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage));
        }
    }
}
