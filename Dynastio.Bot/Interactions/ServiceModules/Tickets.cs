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
        [RequireBotUserPermission(BotUserPermission.CreateTicket)]
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
        [RequireBotUserPermission(BotUserPermission.CreateTicket)]
        [ModalInteraction("tickets.start:*", true)]
        public async Task add(ulong _channel, TicketForm form)
        {
            await DeferAsync(true);

            var channel = Context.Guild.GetTextChannel(_channel);

            var thread = await channel.CreateThreadAsync(Context.User.Username, ThreadType.PrivateThread, ThreadArchiveDuration.ThreeDays, null, false, 0);

            var message = await thread.SendMessageAsync(
                $"## {form.Title1} - <@{Context.User.Id}>:\n" +
                $"{form.Description}\n\n" +
                $"<@&480954902005415937>",

                embed: new EmbedBuilder()
                {
                    Title = "User details",
                    Description = (BotUser.Accounts?.ToStringTable(new string[] { "#", this["account"] + " |", "Id |" },
                                      a => BotUser.Accounts.IndexOf(a) + 1,
                                      a => a.Reminder,
                                      a => a.Id) +
                                      "                 ").ToMarkdown()

                                      ?? this["no_account_found"].ToMarkdown(),
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                        Url = "https://discord.com/channels/@me/" + Context.User.Id,
                    },
                    Color = Color.Orange,
                    Timestamp = DateTime.UtcNow,
                    Footer = new EmbedFooterBuilder() { Text = "Dynast.io Tickets", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() }
                }.Build(),

                components: new ComponentBuilder()
              .WithButton("Delete", $"btn.ticket:unrelated:{channel.Id}:{thread.Id}", ButtonStyle.Danger)
              .WithButton("Close", $"btn.ticket:close:{channel.Id}:{thread.Id}", ButtonStyle.Primary)
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
            [ModalTextInput("description", TextInputStyle.Paragraph, "what is the problem, explain here.", 0, maxLength: 2000, null)]
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

            async Task<IUser> GetTicketOnwer()
            {
                var msgs = await thread.GetPinnedMessagesAsync();

                var botPinedMessage = msgs
                   .FirstOrDefault(a => a.Author.Id == Context.Client.CurrentUser.Id);

                if (botPinedMessage is null) return null;

                return botPinedMessage.MentionedUsers
                    .FirstOrDefault();
            }

            var user = await GetTicketOnwer();
            switch (action)
            {
                case "close":

                    await user.SendMessageAsync(
                       $"## Ticket Closed \n" +
                   $"- Your ticket <#{_thread}> closed by {userMention} probably answered due to unrelated content.\n" +
                   $"> Ваша заявка закрыта пользователем {userMention} из-за содержания, не связанного с ней.\n" +
                   $"\n- If you want to report a bug or you have a suggetion that is not harmful, send message in ⁠ <#1098263349873082438>/ <#1098322508459028480> channel.\n" +
                   $"> Если вы хотите сообщить об ошибке или у вас есть предложение, которое не является вредным, отправьте сообщение в канал <#1098603826291941476>/ <#1098609722006970439>."
                   ).TryAsync();

                    await thread.SendMessageAsync(
                        $"# Ticket Closed \n" +
                        $"- The ticket has been closed by {userMention}, you have access to the history always.\n" +
                        $"- you can ask the admin to reopen this ticket again.");

                    await thread.ModifyAsync(a => { a.Locked = true; a.Archived = true; });

                    await channel.SendMessageAsync($"{user.Mention} > <#{thread}> ticket closed by {Context.User.Mention} !", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                    break;

                case "unrelated":

                    await user.SendMessageAsync(
                        $"## Ticket Deleted\n" +
                    $"- Your ticket closed by {userMention} due to unrelated content.\n" +
                    $"> Ваша заявка закрыта пользователем {userMention} из-за содержания, не связанного с ней.\n" +
                    $"\n- If you want to report a bug or you have a suggetion that is not harmful, send message in ⁠ <#1098263349873082438>/ <#1098322508459028480> channel.\n" +
                    $"> Если вы хотите сообщить об ошибке или у вас есть предложение, которое не является вредным, отправьте сообщение в канал <#1098603826291941476>/ <#1098609722006970439>."
                    ).TryAsync();

                    await thread.DeleteAsync();

                    await channel.SendMessageAsync($"{user.Mention} > <#{thread}> ticket deleted by {Context.User.Mention} !", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                    break;

                case "leave":
                    await thread.RemoveUserAsync(Context.User as IGuildUser);
                    break;

            }
        }

    }
}
