using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Dynastio;
using Discord.Interactions;
using Discord;

namespace Dynastio.Bot.Interactions
{

    public class RequireComponentMessageMentionAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(Discord.IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction is not SocketMessageComponent socket)
            {
                return PreconditionResult.FromError("Interaction is not SocketMessageComponent");
            }

            if (!socket.Message.MentionedUsers.Any(a => a.Id == context.User.Id))
            {
                return PreconditionResult.FromError("controlled-errors:access");
            }

            return PreconditionResult.FromSuccess();
        }

    }

}
