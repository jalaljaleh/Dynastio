using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions
{
    public class RequireMessageComponentOwnerAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        public override string ErrorMessage => "Not yours, do /menu to create your own !";
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction.Type == InteractionType.MessageComponent)
            {
                if (context.Interaction is SocketMessageComponent component)
                {
                    if (component.Message.Interaction.User.Id == context.Interaction.User.Id)
                        return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError(ErrorMessage));

        }
    }


}
