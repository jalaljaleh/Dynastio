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

namespace Discord.Interactions
{
    public class RequireUsageTime : PreconditionAttribute
    {
        private readonly int time;
        public RequireUsageTime(int FromSecounds)
        {
            this.time = FromSecounds;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction.Type is InteractionType.ModalSubmit or InteractionType.MessageComponent)
            {
                var time = (context.Interaction as IComponentInteraction).Message.EditedTimestamp ?? (context.Interaction as IComponentInteraction).Message.Timestamp;
                if ((DateTime.UtcNow - time).TotalSeconds > this.time)
                    return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? $"**Usage Timeout**, This menu have been closed due to inactivity."));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
