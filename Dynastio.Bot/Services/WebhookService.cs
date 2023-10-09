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
using static Dynastio.Bot.SavedChannels;
using static Dynastio.Bot.GuildService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot
{
    public class WebhookService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GuildService _guildService;

        public WebhookService(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _guildService = services.GetRequiredService<GuildService>();
         

            _discord.Ready += _discord_Ready;
        }
        public enum WebhookChannels
        {
            DeleteMessage,
            EditedMessage,
            Timeout,
            Reward
        }
        Dictionary<WebhookChannels, DiscordWebhookClient> _clients = new();
        private async Task _discord_Ready()
        {
            await AddClient(WebhookChannels.DeleteMessage, GuildChannelType.DeletedMessages);
            await AddClient(WebhookChannels.EditedMessage, GuildChannelType.EditedMessages);
            await AddClient(WebhookChannels.Timeout, GuildChannelType.TimeOut);
            await AddClient(WebhookChannels.Reward, GuildChannelType.RewardChannel);
        }

        private async Task AddClient(WebhookChannels webhooktype, GuildChannelType channelType)
        {
            var channelId = _guildService.GetChannelId(channelType);
            var channel = (ITextChannel) await _discord.GetChannelAsync(channelId);
            var webhook = await ChannelUtilities.GetWebhookAsync(channel);
            _clients.Add(webhooktype, new DiscordWebhookClient(webhook));
        }
        public async Task<ulong> LogRewardAsync(string text = null, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null)
        {
            if (!_clients.TryGetValue(WebhookChannels.Reward, out DiscordWebhookClient client)) return 0;

            return await client.SendMessageAsync(text, isTTS, embeds, username, avatarUrl, options, allowedMentions, components, flags, threadId, threadName);
        }
        public async Task<ulong> LogTimeOutAsync(Embed embed, IUser moderator)
        {
            if (!_clients.TryGetValue(WebhookChannels.Timeout, out DiscordWebhookClient client)) return 0;

            return await client.SendMessageAsync(
                 text: "",
                 embeds: new Embed[] { embed },
                 username: moderator.Username,
                 avatarUrl: moderator.GetAvatarUrl() ?? moderator.GetDefaultAvatarUrl());
        }
        public async Task<ulong> LogDeleteMessageAsync(IMessage message, IGuildChannel channel)
        {
            if (!_clients.TryGetValue(WebhookChannels.DeleteMessage, out DiscordWebhookClient client)) return 0;

            var logs = await channel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted);
            var deleteAction = logs.FirstOrDefault(a => (a.Data as MessageDeleteAuditLogData).Target.Id == message.Author.Id);

            return await client.SendMessageAsync(
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
                 avatarUrl: message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl());
        }
        public async Task<ulong> LogEditedMessageAsync(SocketMessage newMessage, IMessage oldMessage, IMessageChannel channel)
        {
            if (!_clients.TryGetValue(WebhookChannels.EditedMessage, out DiscordWebhookClient client)) return 0;

            return await client.SendMessageAsync(
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
                     avatarUrl: newMessage.Author.GetAvatarUrl() ?? newMessage.Author.GetDefaultAvatarUrl());
        }
      
    }
}
