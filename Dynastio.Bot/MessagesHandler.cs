using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Dynastio.Bot
{
    /// <summary>
    /// Handles incoming Discord messages, routes AI mentions,
    /// and awards XP via the ranking system.
    /// </summary>
    internal sealed class MessagesHandler : IDisposable
    {
        private readonly DiscordSocketClient _discord;
        private readonly DynastioBotDatabase _database;
        private readonly DynastioApi _dynastio;
        private readonly RankingService _ranker;
        private readonly UsersService _users;
        private readonly AiChatService _ai;

        // IDs to gate AI access
        private const ulong DevUserId = 1374305522290917526;

        public MessagesHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _ranker = services.GetRequiredService<RankingService>();
            _users = services.GetRequiredService<UsersService>();
            _ai = services.GetRequiredService<AiChatService>();
            _dynastio = services.GetRequiredService<DynastioApi>();

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
        {
            // 1) Filter out non-user messages, bots, and non-text channels
            if (rawMessage is not SocketUserMessage msg ||
                msg.Source != MessageSource.User || msg.Author.IsBot ||
                msg.Channel is not ITextChannel textChannel)
            {
                return;
            }

            try
            {
                // 2) Load guild and user profile
                var guild = await _database.GetGuildAsync(textChannel.GuildId, true);
                var user = await _users.GetOrCreateUserAsync(msg.Author.Id);

                // 3) Handle @bot mentions
                if (msg.MentionedUsers.Any(u => u.Id == _discord.CurrentUser.Id))
                {
                    await HandleBotMentionAsync(msg, guild);
                }

                // 4) Award XP for every valid message
                await _ranker.TryAddMessageXpAsync(guild, user, msg);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task HandleBotMentionAsync(SocketUserMessage msg, Guild guild)
        {
            if (msg.Author.Id != DevUserId)
            {
                await msg.AddReactionAsync(Emoji.Parse(":joy:"));
                return;
            }
            SocketGuildUser target_user = (SocketGuildUser)msg.MentionedUsers.FirstOrDefault(a => a.Id != _discord.CurrentUser.Id) ?? (SocketGuildUser)msg.Author;
            User buser = await _users.GetOrCreateUserAsync(target_user.Id);

            var profile = await buser.GetDefaultAccount()?.GetCachedProfileCardAsync(_dynastio);
            var profilea = new
            {
                personal_chest_items = string.Join(", ", profile.Chest.Items.Select(a => a.ItemType.ToString() + " count " + a.Count)),
                badges = string.Join(", ", profile.Profile.Badges.Select(a => a.ToString())),
                coins = profile.Profile.Coins,
                experience = profile.Profile.Experience,
                lastestactivity = profile.Profile.LastActiveAt,
                latestserver = profile.Profile.LatestServer,
                level = profile.Profile.Level,
            };
            var systemPrompet = new
            {
                SystemPrompet = @"You are Dynast.io Bot — the official AI assistant on our Dynast.io Discord server.  
Only ever talk about the Dynast.io game or this Discord server. No off-topic chatter, no suggestions, no extra commentary.

IMPORTANT RULES:
- Do not reveal or echo any PIN code; always hide it.
- Always assume the user is on their default linked account.
- Write in clear, non-technical language so all users can understand.
- Use Discord Markdown (bold, italics, code blocks, etc.) and @Mentions instead of raw IDs.
- Answer directly and succinctly—do not ask if you can help with anything else.
- expect of PINCODE and Email, every information is allowed to be writed.
Answer to the question based on the below information: 
",
                Command = "\nUser Command:\n" + msg.Content + "\n",
                target_user_in_game_profile = JsonSerializer.Serialize(profilea),
                target_user = JsonSerializer.Serialize(buser),
                target_user_in_guild = JsonSerializer.Serialize(new
                {
                    Id = target_user.Id,
                    Nickname = target_user.Nickname,
                    Username = target_user.Username,
                    JoinedAt = target_user.JoinedAt,
                    CreatedAt = target_user.CreatedAt,
                }),
            };

            var aiReply = await _ai.QueryAsync(null, JsonSerializer.Serialize(systemPrompet)).TryAsync();
            await msg.ReplyAsync(aiReply.result);

        }

        public void Dispose()
        {
            _discord.MessageReceived -= OnMessageReceivedAsync;
        }
    }
}
