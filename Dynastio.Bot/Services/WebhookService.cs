using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Dynastio.Bot.Utilities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Dynastio.Bot.GuildService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot
{
    internal class WebhookService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GuildService _guildService;

        public WebhookService(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _guildService = services.GetRequiredService<GuildService>();

        }

        public async Task InitializeAsync()
        {
            await AddClient(WebhookChannels.DeleteMessage, GuildChannelType.LoggerChannel, ChannelThreadType.DeletedMessages);
            await AddClient(WebhookChannels.EditedMessage, GuildChannelType.LoggerChannel, ChannelThreadType.EditedMessages);
        }
        private async Task AddClient(WebhookChannels webhooktype, GuildChannelType channelType, ChannelThreadType thread)
        {
            var channelId = _guildService.GetChannelId(channelType);
            var channel = (IForumChannel)await _discord.GetChannelAsync(channelId);
            var webhook = await ChannelUtilities.GetWebhookAsync(channel);
            _clients.Add(webhooktype, (new DiscordWebhookClient(webhook), _guildService.GetChanneThreadlId(thread)));
        }
        Dictionary<WebhookChannels, (DiscordWebhookClient client, ulong threadId)> _clients = new();
        public async Task<ulong> LogDeleteMessageAsync(IMessage message, IGuildChannel channel)
        {
            if (!_clients.TryGetValue(WebhookChannels.DeleteMessage, out (DiscordWebhookClient client, ulong threadId) value)) return 0;

            var logs = await channel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted);
            var deleteAction = logs.FirstOrDefault(a => (a.Data as MessageDeleteAuditLogData).Target.Id == message.Author.Id);

            return await value.client.SendMessageAsync(
                 text: "",
                 embeds: new Embed[] {
                         new EmbedBuilder() {
                                Description = $"{message.Author.Id.ToUserMention()} > {channel.Id.ToChannelMention()} > {message.CreatedAt.UtcDateTime.ToDiscordUnixTimestampFormat()}",
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    .WithName("Message")
                                    .WithValue(message.Content),

                                    new EmbedFieldBuilder()
                                    .WithName("Moderator")
                                    .WithValue(deleteAction is not null ? deleteAction.User.Id.ToUserMention() : "` deleted by user `")

                                }}.Build()
                 },
                 username: message.Author.Username,
                 avatarUrl: message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl(),
                 threadId: value.threadId);
        }
        public async Task<ulong> LogEditedMessageAsync(SocketMessage newMessage, IMessage oldMessage, IMessageChannel channel)
        {
            if (!_clients.TryGetValue(WebhookChannels.EditedMessage, out (DiscordWebhookClient client, ulong threadId) value)) return 0;

            return await value.client.SendMessageAsync(
                     text: "",
                     embeds: new Embed[] {
                            new EmbedBuilder() {
                                Description = $"{newMessage.Author.Id.ToUserMention()} > {channel.Id.ToChannelMention()} > {oldMessage.CreatedAt.UtcDateTime.ToDiscordUnixTimestampFormat()}",
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    .WithName("Old message")
                                    .WithValue(oldMessage.Content),
                                    new EmbedFieldBuilder()
                                    .WithName("New message")
                                    .WithValue(newMessage.Content)
                                }}.Build()
                     },
                     username: newMessage.Author.Username,
                     avatarUrl: newMessage.Author.GetAvatarUrl() ?? newMessage.Author.GetDefaultAvatarUrl(),
                     threadId: value.threadId);
        }
        public enum WebhookChannels
        {
            DeleteMessage,
            EditedMessage
        }
    }
}
