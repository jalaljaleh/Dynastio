using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Dynastio.Extenstions;
using Discord.Rest;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Logs.Services;
using Discord.Webhook;

namespace Dynastio.Bot.Handlers
{
    internal class MessagesHandler
    {
        readonly DiscordSocketClient _discord;
        readonly ulong deleteMessagecahnnelId;
        public MessagesHandler(DiscordSocketClient discord, ulong deleteMessagecahnnelId)
        {
            this._discord = discord;
            this._discord.MessageDeleted += _discord_MessageDeleted;
            this.deleteMessagecahnnelId = deleteMessagecahnnelId;
        }
        private DiscordWebhookClient webhook_deleteMessages;
        private IGuildChannel loggerChannel;
        private async Task _discord_MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            if (channel.HasValue && channel.Value is IGuildChannel guildChannel)
            {
                var message = await cachedMessage.GetOrDownloadAsync();
                if (message is null || message.Source != MessageSource.User)
                    return;

                if (message is null) return;

                var logs = await guildChannel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted);
                var deleteAction = logs.FirstOrDefault(a => ((MessageDeleteAuditLogData)a.Data).Target.Id == message.Author.Id);


                var embed = new EmbedBuilder()
                {
                    Description = $"{message.Author.Mention} > <#{channel.Id}> > {message.CreatedAt.UtcDateTime.UnixTimestampDiscordFormat()}",
                    ThumbnailUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    Fields = new List<EmbedFieldBuilder>()
                                        {
                                            new EmbedFieldBuilder()
                                            .WithName("Message")
                                            .WithValue(message.Content),

                                            new EmbedFieldBuilder()
                                            .WithName("Moderator")
                                            .WithValue(deleteAction is not null ? deleteAction.User.Mention : "` deleted by user `")

                                        }
                }.Build();

                if (this.webhook_deleteMessages is null)
                {
                    if (loggerChannel is null)
                        loggerChannel = await guildChannel.Guild.GetTextChannelAsync(deleteMessagecahnnelId);

                    webhook_deleteMessages = new DiscordWebhookClient(await WebhookService.GetWebhookAsync((ITextChannel)loggerChannel));
                }
                await webhook_deleteMessages.SendMessageAsync(
                    text: ($"[Show Profile](discord://-/users/{message.Author.Id})"),
                    username: message.Author.Username,
                    avatarUrl: message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                    embeds: new Embed[] { embed });
            }
        }
    }
}
