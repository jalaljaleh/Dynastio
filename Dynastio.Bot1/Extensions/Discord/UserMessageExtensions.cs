using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions;

namespace Dynastio.Bot
{
    public static class UserMessageExtensions
    {     
        public static async Task WhenNoResponse(this IUserMessage message, CustomSocketInteractionContext context, TimeSpan timeout, Action<IUserMessage> action)
        {
            var userResponse = await context.ReadMessageComponentFromMessageAsync(message, timeout);
            if (userResponse is null)
            {
                action.Invoke(message);
            }
        }
    }
}
