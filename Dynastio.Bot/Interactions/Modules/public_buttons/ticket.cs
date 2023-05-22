using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.public_buttons
{
    public class TicketModule : CustomInteractionModuleBase
    {
        [RateLimit(999999999)]
        [ComponentInteraction("btn.public.ticket.start")]
        public async Task start()
        {
            await DeferAsync();

            var thread = await Context.Guild
                .GetTextChannel(480951565255966720)
                .CreateThreadAsync(Context.User.Username, ThreadType.PrivateThread, ThreadArchiveDuration.OneWeek, null, false, 0);

            await thread.SendMessageAsync(
                $"**Important**" +
                $"This is a safe and private thread with Dynastio Staff **No Admin, No Moderator**.\n" +
                $"> Это безопасный и конфиденциальный поток с персоналом Dynastio ** Без администраторов, без модераторов**.\n\n\n" +
                $"**Notes:**\n" +
                $"- Do not mention anyone.\n\n" +
                $"> Примечания: \n" +
                $"-Не упоминайте никого.\n\n" +
                $"");

            await thread.SendMessageAsync(
              $"<@&480954902005415937> Send Your Message to <@&480954902005415937>:");
        }

    }
}
