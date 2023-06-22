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

namespace Dynastio.Bot.Interactions.ServiceModules
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

        [RateLimit(30)]
        [ComponentInteraction("btn.ticket.start:*")]
        public async Task btn_ticket_start(ulong _channel)
        {
            var modal = new ModalBuilder(this["modal.ticket.start.title"], $"tickets.start:{_channel}")
               .AddTextInput(new TextInputBuilder("Title", "title", TextInputStyle.Short, "its a name for your ticket.", 0, maxLength: 60, null))
               .AddTextInput(new TextInputBuilder("Description", "description", TextInputStyle.Paragraph, "what is the problem, explain here.", 0, maxLength: 1000, true))
              .Build();

            await RespondWithModalAsync(modal);
        }
        [RateLimit(10)]
        [ModalInteraction("tickets.start:*", true)]
        public async Task add(ulong _channel, TicketForm form)
        {
            await DeferAsync(true);

            var channel = Context.Guild.GetTextChannel(_channel);

            var thread = await channel.CreateThreadAsync(Context.User.Username + $"-{form.Title1}", ThreadType.PrivateThread, ThreadArchiveDuration.ThreeDays, null, false, 0);

            var message = await thread.SendMessageAsync(
                $"## Important\n" +
                $"- This is a safe and private thread with Dynastio Staff **No Admin, No Moderator**.\n" +
                $"> Это безопасный и конфиденциальный поток с персоналом Dynastio ** Без администраторов, без модераторов**.\n\n\n" +
                $"## Notes:\n" +
                $"- Do not mention anyone, Do not Invite anyone, Do not use Add buttons if you know nothing about them.\n" +
                $"> Не упоминайте никого." +
                $"- Use the close button to close the ticket when you done." +
                $"\n\n" +
                $"<@&480954902005415937>" +
                $"> **<@{Context.User.Id}> Send Your Message:**",

                components: new ComponentBuilder()
              .WithButton("Unrelated", $"btn.ticket:unrelated:{channel.Id}:{thread.Id}", ButtonStyle.Danger)
              .WithButton("Close", $"btn.ticket:close:{channel.Id}:{thread.Id}", ButtonStyle.Danger)
              .Build());

            await message.PinAsync();

            await FollowupAsync($"## Done\nYour ticket created, click here: {thread.Mention}.", ephemeral: true);

        }
        public class TicketForm : IModal
        {
            public string Title => "Ticket";

            [InputLabel("Title")]
            [RequiredInput(true)]
            [ModalTextInput("title", TextInputStyle.Short, "its a name for your ticket.", 0, maxLength: 60, null)]
            public string Title1 { get; set; }

            [InputLabel("Description")]
            [RequiredInput(true)]
            [ModalTextInput("description", TextInputStyle.Paragraph, "what is the problem, explain here.", 0, maxLength: 1000, null)]
            public string Description { get; set; }

        }
        [RateLimit(5, 2)]
        [RequireUserPermission(ChannelPermission.ManageThreads)]
        [ComponentInteraction("btn.ticket:*:*:*")]
        public async Task btn_ticket(string action, ulong _channel, ulong _thread)
        {
            await DeferAsync();

            var channel = Context.Guild.GetTextChannel(_channel);
            if (channel is null) return;

            var thread = channel.Threads.FirstOrDefault(a => a.Id == _thread);
            if (thread is null) return;
            async Task SendMessageToTicketOwnerAsync(string msg)
            {
                var msgs = await thread.GetPinnedMessagesAsync();

                var botPinedMessage = msgs
                    .FirstOrDefault(a => a.Author.Id == Context.Client.CurrentUser.Id);

                if (botPinedMessage is null) return;

                var user = botPinedMessage.MentionedUsers
                    .FirstOrDefault();

                if (user is null) return;

                await user.SendMessageAsync(msg).TryAsync();
            }

            switch (action)
            {
                case "close":

                    await SendMessageToTicketOwnerAsync(
                       $"## Ticket Closed\n" +
                   $"- Your ticket closed by {userMention} probably due to unrelated content.\n" +
                   $"> Ваша заявка закрыта пользователем {userMention} из-за содержания, не связанного с ней.\n" +
                   $"\n- If you want to report a bug or you have a suggetion that is not harmful, send message in ⁠ <#1098263349873082438>/ <#1098322508459028480> channel.\n" +
                   $"> Если вы хотите сообщить об ошибке или у вас есть предложение, которое не является вредным, отправьте сообщение в канал <#1098603826291941476>/ <#1098609722006970439>."
                   ).TryAsync();

                    await thread.SendMessageAsync($"## Closed\nThe thread has been closed by {userMention}.");
                    await thread.ModifyAsync(a => { a.Locked = true; a.AutoArchiveDuration = ThreadArchiveDuration.OneDay; });
                    break;

                case "unrelated":

                    await SendMessageToTicketOwnerAsync(
                        $"## Ticket Closed\n" +
                    $"- Your ticket closed by {userMention} due to unrelated content.\n" +
                    $"> Ваша заявка закрыта пользователем {userMention} из-за содержания, не связанного с ней.\n" +
                    $"\n- If you want to report a bug or you have a suggetion that is not harmful, send message in ⁠ <#1098263349873082438>/ <#1098322508459028480> channel.\n" +
                    $"> Если вы хотите сообщить об ошибке или у вас есть предложение, которое не является вредным, отправьте сообщение в канал <#1098603826291941476>/ <#1098609722006970439>."
                    ).TryAsync();

                    await thread.DeleteAsync();
                    break;

                case "leave":
                    await thread.RemoveUserAsync(Context.User as IGuildUser);
                    break;

            }
        }

    }
}
