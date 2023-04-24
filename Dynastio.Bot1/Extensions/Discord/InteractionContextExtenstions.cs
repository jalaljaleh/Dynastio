using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class InteractionContextExtenstions
    {
        public static string UserMention(this IInteractionContext context)
        {
            return context.User.Id.ToUserMention();
        }
    }
}
