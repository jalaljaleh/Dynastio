//using Discord;
//using Dynastio.Bot.Handlers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Dynastio.Bot.Events
//{
//	internal class guilds_message_deleted : HandlersBase
//	{
//		public guilds_message_deleted(IServiceProvider services) : base(services)
//		{
//            _discord.MessageDeleted += _discord_MessageDeleted;
//        }

//        private async Task _discord_MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
//        {
//            if (_messageloggerBannedChannels.Contains(channel.Id)) return;

//            if (channel.HasValue && channel.Value is IGuildChannel guildChannel)
//            {
//                if (guildChannel.GuildId != SavedGuilds.OfficialGuild) return;

//                var message = await cachedMessage.GetOrDownloadAsync();
//                if (message is null || message.Source != MessageSource.User)
//                    return;

//                if (message is null) return;

//                var logs = await channel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted);
//                var deleteAction = logs.FirstOrDefault(a => (a.Data as MessageDeleteAuditLogData).Target.Id == message.Author.Id);

//               await client.SendMessageAsync(
//                     text: "",
//                     embeds: new Embed[] {
//                         new EmbedBuilder() {
//                                Description = $"{message.Author.Id.ToUserMention()} > {channel.Id.ToChannelMention()} > {message.CreatedAt.UtcDateTime.ToDiscordUnixTimestampFormat()}",
//                                Fields = new List<EmbedFieldBuilder>()
//                                {
//                                    new EmbedFieldBuilder()
//                                    .WithName("Message")
//                                    .WithValue(message.Content),

//                                    new EmbedFieldBuilder()
//                                    .WithName("Moderator")
//                                    .WithValue(deleteAction is not null ? deleteAction.User.Id.ToUserMention() : "` deleted by user `")

//                                }}.Build()
//                     },
//                     username: message.Author.Username,
//                     avatarUrl: message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl());
//            }
//        }
//    }
//}
