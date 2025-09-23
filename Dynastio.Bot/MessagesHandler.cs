using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Global.Helper;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Dynastio.Bot
{
    /// <summary>
    /// Central message router:
    /// 1. Filters out irrelevant messages (bots, DMs, non‐text channels).  
    /// 2. Dispatches commands.  
    /// 3. Invokes AI replies for developer mentions.  
    /// 4. Awards XP for ordinary messages.  
    /// </summary>
    internal sealed class MessagesHandler : IDisposable
    {
        private readonly DiscordSocketClient _discord;
        private readonly DynastioBotDatabase _database;
        private readonly UsersService _users;
        private readonly RankingService _ranker;
        private readonly ClientService _clientService;
        private readonly CommandHandlerService _commandHandler;
        private readonly CommandService _commands;
        private readonly AiChatService _aiChat;
        private readonly DynastioApi _dynastioApi;
        private readonly TelegramBotService _telegram;
        private readonly IServiceProvider _services;

        public MessagesHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _users = services.GetRequiredService<UsersService>();
            _ranker = services.GetRequiredService<RankingService>();
            _clientService = services.GetRequiredService<ClientService>();
            _commandHandler = services.GetRequiredService<CommandHandlerService>();
            _aiChat = services.GetRequiredService<AiChatService>();
            _dynastioApi = services.GetRequiredService<DynastioApi>();
            _commands = services.GetRequiredService<CommandService>();
            _telegram = services.GetRequiredService<TelegramBotService>();

            _services = services;
            // Subscribe to all incoming messages
            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        /// <summary>
        /// Entry point for every Discord message. Filters, routes commands or AI, and awards XP.
        /// </summary>
        /// 
        //if (raw.Channel.Id == 480951629978533898) //┊〢📢│𝐀𝐧𝐧𝐨𝐮𝐧𝐜𝐞𝐦𝐞𝐧𝐭𝐬
        //{
        //    if (raw.Attachments.Count == 0)
        //    {
        //        await _telegram.SendMessageAsync("@DynastioBot", raw.CleanContent);
        //    }
        //    else
        //    {
        //        await _telegram.SendPhotoMessageAsync("@DynastioBot", raw.Attachments.First().Url, raw.Content);
        //    }
        //    return;
        //}

        private async Task OnMessageReceivedAsync(SocketMessage raw)
        {
            // 1. Ignore system/bot messages
            if (raw.Source != MessageSource.User || raw is not SocketUserMessage message) return;
            if (raw.Channel is IDMChannel dMChannel)
            {
                await OnDmMessageAsync(message, dMChannel);
                return;
            }

            // 1. Ignore DMs, and non-text channels
            if (message.Channel is not IGuildChannel guildChannel)
                return;

            Program.UnsafeCode = true;
            if (guildChannel.GuildId != 480416088312774657)
                return;

            await OnTeamOwnerMentionedAsync(message);

            try
            {
                var user = await _users.GetOrCreateUserAsync(message.Author.Id).ConfigureAwait(false);
                if (user.IsBanned)
                    return;

                var guild = await _database.GetGuildAsync(guildChannel.GuildId).ConfigureAwait(false);

                var cmdResult = await _commandHandler.ExecuteCommandAsync(message, guild, user).ConfigureAwait(false);
                if (cmdResult is null)
                {
                    await _ranker.TryAddMessageXpAsync(guild, user, message).ConfigureAwait(false);
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing incoming message");
            }

        }
        private async Task OnDmMessageAsync(SocketUserMessage dmMessage, IDMChannel dMChannel)
        {
            var owner = _clientService.GetApplicationTeamOwner();
            if (dmMessage.Author.Id == owner.Id)
            {
                if (dmMessage.ReferencedMessage is null)
                {
                    await dmMessage.ReplyAsync("refrence message not found.");
                    return;
                }
                if (TextMatching.TryGetUserId(dmMessage.ReferencedMessage.Content, out ulong userid) is false)
                {
                    await dmMessage.ReplyAsync("target user not found in refrence message.");
                    return;
                }

                var targetUser = await _discord.GetUserAsync(userid);
                if (targetUser is null)
                    await dmMessage.ReplyAsync("can't find user !");

                var sendMe = await targetUser.SendMessageAsync(dmMessage.Content).TryAsync();
                if (sendMe.isSuccessful is false)
                    await dMChannel.SendMessageAsync("can't send message !");
                else
                    await dMChannel.SendMessageAsync("message sent to user !");

                return;
            }
            await owner.SendMessageAsync($"# New message from: {dmMessage.Author.Mention}\n" + dmMessage.Content);
        }
        private async Task OnTeamOwnerMentionedAsync(SocketUserMessage message)
        {
            var owner = _clientService.GetApplicationTeamOwner();

            var text = message.Content.ToLowerInvariant();

            string[] triggers = { owner.GlobalName.ToLower(), owner.Mention.ToLower(), owner.Id.ToString(), "джалеху", "Жалех", "jale" };

            if (triggers.Any(trigger => text.Contains(trigger)))
                await message.AddReactionAsync(new Emoji("👀"));

            else if (message.ReferencedMessage != null && message.ReferencedMessage.Author.Id == owner.Id)
                await message.AddReactionAsync(new Emoji("👀"));
        }
        /// <summary>
        /// Unsubscribe from events to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _discord.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}
