//using Discord.Interactions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord;
//using Dynastio.Bot.Global;
//using Dynastio.Net;
//using Dynastio.Data;
//using ZstdSharp.Unsafe;
//using Discord.WebSocket;
//using Discord.Rest;
//using Dynastio.Bot.Interactions.Forms;
//using Dynastio.Bot.Utilities;

//namespace Dynastio.Bot.Interactions.modules.moderators
//{
//    [EnabledInDm(false)]
//    [RequireContext(ContextType.Guild)]
//    [RequireSpecialRole(SavedRoles.RoleType.Moderator)]
//    public class messagesModule : CustomInteractionModuleBase
//    {
//        public DynastioData _dynastioData { get; set; }
//        public WebhookService _webhookService { get; set; }

//        [MessageCommand("Warn Message")]
//        [RequireUserPermission(ChannelPermission.SendMessages)]
//        public async Task WarnMessageAsync(IMessage message)
//        {
//            if (SavedUsers.Develoeprs.Contains(message.Author.Id)) return;

//            var modal = new ModalBuilder("Warn Message", $"mod.messages.warn:{message.Id}")
//               .AddTextInput(new TextInputBuilder("Reason", "1", TextInputStyle.Paragraph, "Rule 1", 2, 500, true, null))
//                .Build();
//            await RespondWithModalAsync(modal);
//        }
//        [ModalInteraction("mod.messages.warn:*", true)]
//        public async Task ModalWarnMessageAsync(ulong messageId, GenericInputModal<string> modal)
//        {
//            await DeferAsync();

//            var targetMessage = await Context.Channel.GetMessageAsync(messageId);
//            var targetUser = targetMessage.Author;
//            string reason = modal.First;

//            var embed = new EmbedBuilder()
//            {
//                Title = "Warning .. !",
//                Description = $"{targetUser.Mention} You have been warned by {userMention} for {targetMessage.GetJumpUrl()}.",
//                ThumbnailUrl = targetUser.GetAvatarUrl() ?? targetUser.GetDefaultAvatarUrl(),
//                Color = Color.DarkOrange,
//                Fields = new List<EmbedFieldBuilder>()
//                    {
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Reason",
//                            Value = "` " + reason.TryRemove(40,false) + " `",
//                            IsInline = true
//                        },
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Moderator",
//                            Value = userMention,
//                            IsInline = true
//                        }
//                    }
//            };


//            await FollowupAsync(
//                text: targetUser.Mention + " | " + userMention,
//                embed: embed.Build());

//            await Task.Delay(100);

//            embed.Title = $"{targetUser.Username} got a warn from moderators !";
//            embed.Description = 
//                $"### **{targetUser.Mention}:**\n" +
//                $"{targetMessage.Content.TryRemove(3800)}\n" +
//                $"Attachments: {targetMessage.Attachments.Count}";

//            var log = SavedChannels.Get(SavedChannels.GuildChannelType.ModeratorActions);
//            await Context.Guild.GetTextChannel(log).SendMessageAsync(
//                 embed: embed.Build());

//            // update user
//            var targetBotUser = await _dynastioData.GetUserAsync(targetUser.Id);
//            targetBotUser.Warns.Add(new Data.UserWarn()
//            {
//                Content = reason,
//                CreatedAt = DateTime.UtcNow,
//                SourceId = Context.User.Id
//            });
//            await _dynastioData.UpdateAsync(targetBotUser);
//        }


//        [MessageCommand("Delete Messages")]
//        [RequireUserPermission(ChannelPermission.SendMessages)]
//        public async Task DeleteMessageAsync(IMessage message)
//        {
//            if (SavedUsers.Develoeprs.Contains(message.Author.Id)) return;

//            var modal = new ModalBuilder("Delete Message", $"mod.messages.delete:{message.Id}")
//               .AddTextInput(new TextInputBuilder("Count", "1", TextInputStyle.Short, "How many message should be delete of this user ?", 1, 2, true, "1"))
//               .AddTextInput(new TextInputBuilder("Reason", "2", TextInputStyle.Paragraph, "Rule 1", 2, 500, true, null))
//                .Build();
//            await RespondWithModalAsync(modal);
//        }
//        [ModalInteraction("mod.messages.delete:*", true)]
//        public async Task ModalDeleteMessageAsync(ulong messageId, GenericInputModal<int, string> modal)
//        {
//            await DeferAsync(true);

//            var targetMessage = await Context.Channel.GetMessageAsync(messageId);
//            var targetUser = targetMessage.Author;

//            int count = modal.First;
//            string reason = modal.Second;

//            IEnumerable<IMessage> messages;
//            if (count == 1)
//            {
//                messages = new List<IMessage>() { targetMessage };
//            }
//            else
//            {
//                messages = await targetMessage.Channel.GetMessagesAsync(messageId, Direction.Around, count).FlattenAsync();
//                messages = messages.Where(a => a.Author.Id == targetUser.Id).ToList();
//            }
//            await (targetMessage.Channel as ITextChannel).DeleteMessagesAsync(messages.ToList());

//            await FollowupAsync($"done, {count} messages deleted.");


//            await _webhookService.LogDeleteMessagesAsync(messages, reason, userMention);
//        }
//    }

//}
