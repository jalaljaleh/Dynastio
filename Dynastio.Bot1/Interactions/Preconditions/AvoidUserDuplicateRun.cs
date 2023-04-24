//Originally made by jalaljaleh for Dynastio.Discord
//Read the original file at https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Interactions/Preconditions/RateLimit.cs
//Originally licensed under Apache 2.0 https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt
//Modifications:
// 

namespace Discord.Interactions
{
    using Discord.Interactions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord.WebSocket;
    using Discord;
    using Dynastio;
    using Dynastio.Bot.Interactions;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.DependencyInjection;
    using Discord.Commands;

    public class AvoidUserDuplicateRun : PreconditionAttribute
    {
        public static ConcurrentDictionary<ulong, List<string>> values { get; set; } = new ConcurrentDictionary<ulong, List<string>>();


        // You can delete this part and use your InteractionService.InteractionExecuted event handler then call AvoidUserDuplicateRun.RemoveLimit()
        public static bool IsEventHandlerRegistered = false;
        private Task AvoidDuplicateRun_InteractionExecuted(ICommandInfo commandInfo, IInteractionContext context, IResult arg3)
        {
            RemoveLimit(commandInfo, context);
            return Task.CompletedTask;
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            // You can delete this part and use your InteractionService.InteractionExecuted event handler then call AvoidUserDuplicateRun.RemoveLimit()
            if (!IsEventHandlerRegistered)
            {
                IsEventHandlerRegistered = true;
                services.GetRequiredService<InteractionService>().InteractionExecuted += AvoidDuplicateRun_InteractionExecuted;
            }
            // delete until here.

            ulong id = context.User.Id;
            var contextId = commandInfo.Module.Name + "//" + commandInfo.Name + "//" + commandInfo.MethodName;

            var result = values.GetOrAdd(id, new List<string>() { });
            try
            {
                if (values.Any(a => a.Key == id && a.Value.FirstOrDefault(a => a == contextId) != null))
                {
                    return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage));
                }
            }
            finally
            {
                result.Add(contextId);
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
        public static void RemoveLimit(ICommandInfo commandInfo, IInteractionContext context)
        {

            var target = values.FirstOrDefault(a => a.Key == context.User.Id);
            if (target.Value != null)
            {
                string command = commandInfo.Module.Name + "//" + commandInfo.Name + "//" + commandInfo.MethodName;
                target.Value.Remove(command);

            }
        }


    }
}
