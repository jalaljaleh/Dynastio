using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot.Interactions._Modules
{
    public class TicketModule : CustomInteractionModuleBase
    {
        [RequireBotOwner]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("owner-setup-tickets", "setup ticket module.")]
        public async Task setup(ITextChannel channel, ITextChannel TargetChannel)
        {
            await DeferAsync(true);

            await channel.SendMessageAsync(
             "** Private Ticket **\n" +
             $"- If you have an enquiry that requires discussion with staff, create a ticket by clicking on the button below!\n" +
             $"- Abuse of the ticket system will lead to action being taken by staff members.\n" +
             $"- If you want to report a bug or you have a suggetion that is not harmful, send message in ⁠feedback ⁠report-bug channel.\n\n" +
             $">>> - Если у вас есть запрос, требующий обсуждения с сотрудником, создайте тикет, нажав на кнопку ниже! \n" +
             $"- Злоупотребление системой тикетов приведет к принятию мер сотрудниками.\n" +
             $"- Если вы хотите сообщить об ошибке или у вас есть предложение, которое не является вредным, отправьте сообщение в канал ⁠обратная-связь ⁠сообщить-ошибке.\n" +
             $"\n",
              components: new ComponentBuilder()
              .WithButton("Start", $"btn.ticket.start:{TargetChannel.Id}", ButtonStyle.Success, Emoji.Parse("📩"))
              .Build());

            await FollowupAsync("done");
        }

        [RateLimit(10000000)]
        [ComponentInteraction("btn.ticket.start:*")]
        public async Task btn_ticket_start(ulong channel)
        {
            await DeferAsync();

            var thread = await Context.Guild
                .GetTextChannel(channel)
                .CreateThreadAsync(Context.User.Username, ThreadType.PrivateThread, ThreadArchiveDuration.ThreeDays, null, false, 0);

            await thread.SendMessageAsync(
                $"**Important**\n" +
                $"- This is a safe and private thread with Dynastio Staff **No Admin, No Moderator**.\n" +
                $"> Это безопасный и конфиденциальный поток с персоналом Dynastio ** Без администраторов, без модераторов**.\n\n\n" +
                $"**Notes:**\n" +
                $"- Do not mention anyone.\n" +
                $"> Не упоминайте никого.\n\n" +
                $"<@&480954902005415937>\n\n");

            await thread.SendMessageAsync(
              $"> **<@{Context.User.Id}> Send Your Message:**");
        }


    }
}
