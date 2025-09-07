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
    public class RequireMessageComponentTimeoutAttribute : BotPreconditionAttribute<BotSocketInteractionContext>
    {
        public override string ErrorMessage => "This message component has expired. Please try again or request a new one.";
        private readonly TimeSpan _timeout;

        public RequireMessageComponentTimeoutAttribute(int seconds = 600)
        {
            _timeout = TimeSpan.FromSeconds(seconds);
        }
      //  static ComponentType[] disabledTypes = [ComponentType.TextInput, ComponentType.UserSelect, ComponentType.SelectMenu, ComponentType.RoleSelect, ComponentType.MentionableSelect, ComponentType.ChannelSelect, ComponentType.Button];
        protected override Task<PreconditionResult> CheckRequirementsAsync(BotSocketInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction.Type == InteractionType.MessageComponent && context.Interaction is SocketMessageComponent component)
            {
                var age = DateTime.UtcNow - component.Message.CreatedAt.UtcDateTime;
                if (age > _timeout)
                {
                    return Task.FromResult(PreconditionResult.FromError(ErrorMessage));
                }
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }


}
