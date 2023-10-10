using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Utilities
{
    public class ChannelUtilities
    {
        public static async Task<List<IMessage>> GetChannelMessageAsync(ITextChannel channel, int count, Direction direction = Direction.Before)
        {
            List<IMessage> msgs = new();
            ulong lastMessageId = 0;
            int _count = count > 100 ? 100 : count;
            do
            {
                var channelMessages = lastMessageId is 0

                    ? await channel.GetMessagesAsync(_count - msgs.Count)
                    .FlattenAsync()
                    .TryAsync()

                    : await channel.GetMessagesAsync(lastMessageId, direction, _count - msgs.Count)
                    .FlattenAsync()
                    .TryAsync();

                msgs.AddRange(channelMessages.result);

                var lastmessage = channelMessages.result.LastOrDefault();

                if (msgs.Count >= count || lastmessage is null || lastMessageId == lastmessage.Id)
                {
                    lastMessageId = 0;
                }
                else
                {
                    await Task.Delay(150);
                    lastMessageId = lastmessage.Id;
                }

            } while (lastMessageId != 0);

            return msgs;
        }
        public static async Task SendOrUpdateMessage(ITextChannel channel, ulong botId, string txt,Embed[] embeds = default)
        {
            if (channel == null) return;

            var msgs = await channel.GetMessagesAsync()
                .FlattenAsync()
                .TryAsync();

            IMessage targetMessage = null;
            if (msgs.isSuccesful && msgs.result.Any())
            {
                var filter = msgs.result
                     .Where(a => a.Author.Id == botId)
                     .OrderByDescending(a => a.CreatedAt)
                     .ThenBy(a => a.EditedTimestamp)
                     .ToList();

                targetMessage = filter.FirstOrDefault();

                if (filter.Count > 0)
                    await channel.DeleteMessagesAsync(filter.Skip(1));
            }

            var editionResult = false;
            if (targetMessage is not null)
            {
                editionResult = await (targetMessage as IUserMessage)
                    .ModifyAsync(x =>
                    {
                        x.Content = txt;
                        x.Embed = null;
                        x.Embeds = embeds;
                    })
                    .TryAsync();

                if (editionResult is false)
                    await (targetMessage as IUserMessage).DeleteAsync();
            }
            if (targetMessage is null)
            {
                await channel.SendMessageAsync(txt, allowedMentions: AllowedMentions.None);
                return;
            }
        }
        public static async Task<IWebhook> GetWebhookAsync(ITextChannel channel)
        {
            try
            {
                var webhooks = await channel.GetWebhooksAsync();
                if (webhooks == null || webhooks.Count == 0)
                {
                    var webhook = await channel.CreateWebhookAsync("Dynastio");
                    webhooks.Append(webhook);
                }
                return webhooks.First();
            }
            catch
            {
                return null;
            }
        }
    }
}
