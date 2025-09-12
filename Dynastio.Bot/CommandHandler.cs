using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class CommandHandlerService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly AiChatService _ai;
        private readonly UsersService _users;
        private readonly DynastioBotDatabase _database;
        private readonly DynastioApi _dynastio;
        private readonly RankingService _ranker;
        private readonly IServiceProvider _services;
        public CommandHandlerService(IServiceProvider services)
        {
            _services = services;
            _dynastio = services.GetRequiredService<DynastioApi>();
            _commands = services.GetRequiredService<CommandService>();
            _database = services.GetRequiredService<DynastioBotDatabase>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _ranker = services.GetRequiredService<RankingService>();
            _users = services.GetRequiredService<UsersService>();
            _ai = services.GetRequiredService<AiChatService>();

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            //// if it qualifies as a command.
            //_discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task<IResult> MessageReceivedAsync(SocketUserMessage message)
        {
            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return null;

            var context = new SocketCommandContext(_discord, message);
            return await _commands.ExecuteAsync(context, argPos, _services);
        }

        private const ulong DevUserId = 1374305522290917526;
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
            {
                if (context.Message.Author.Id != DevUserId)
                {
                    await context.Message.AddReactionAsync(Emoji.Parse(":joy:"));
                    return;
                }
                var aiResponse = await ReplyWithAIAsync(context.Message as SocketUserMessage);

                return;
            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            // await context.Channel.SendMessageAsync($"error: {result}");
        }
        // IDs to gate AI access
        private async Task<bool> ReplyWithAIAsync(SocketUserMessage msg)
        {
            SocketGuildUser target_user = (SocketGuildUser)msg.Author;
            User buser = await _users.GetOrCreateUserAsync(target_user.Id);

            var SystemPrompet =
      @"You are **Dynast.io Bot** — the official AI assistant on our Dynast.io Discord server.

IDENTITY & VOICE
- Be friendly, casual, and human‑like — as if you’re chatting with friends.
- Write in clear, simple, non‑technical language.
- Use Discord Markdown for emphasis (**bold**, *italics*, `code blocks`) and @Mentions instead of raw IDs.
- Answer directly and succinctly. Do not add “Can I help with anything else?” at the end.
- When topics touch real life, you may be playful or totally funny if it fits the moment.
- Keep responses natural, varied, and engaging — avoid robotic phrasing.

CONTENT POLICY FOR RESPONSES
- Use @Mentions for people, roles, and channels instead of raw IDs.
- Be witty where appropriate; never snarky or rude. Light teasing is okay if clearly playful.
- Never break character as Dynast.io Bot.

FORMATTING GUIDELINES
- Use concise headings or bold lead‑ins for structure when needed.
- Prefer bullets over long paragraphs.
- Include code blocks for commands or snippets only (e.g., `/link`, `/duel @user`).
- No walls of text; aim for 3–6 short lines unless the owner explicitly asks for depth.

        user prompet: " + msg.Content;


            var reply = await _ai.QueryAsync(null, SystemPrompet).TryAsync();
            if (reply.isSuccessful)
                await msg.ReplyAsync(reply.result);
            else
                await msg.ReplyAsync("I'm sorry, but I can't assist now !");
            return true;
        }






        //        private async Task<bool> ReplyWithAIAsync(SocketUserMessage msg)
        //        {
        //            SocketGuildUser target_user = (SocketGuildUser)msg.MentionedUsers.FirstOrDefault(a => a.Id != _discord.CurrentUser.Id) ?? (SocketGuildUser)msg.Author;
        //            User buser = await _users.GetOrCreateUserAsync(target_user.Id);

        //            var SystemPrompet = 
        //@"You are Dynast.io Bot — the official AI assistant on our Dynast.io Discord server.
        //IMPORTANT RULES:
        //- Always assume the user is on their default linked account & Do not reveal or echo any PIN code; always hide it &expect of PINCODE and Email, every information is allowed to be writed.
        //- Write in clear, non-technical language so all users can understand.
        //- Use Discord Markdown (bold, italics, code blocks, etc.) and @Mentions instead of raw IDs.
        //- Answer directly and succinctly—do not ask if you can help with anything else.
        //Command = \nUser writed:\n" + msg.Content + "\n";

        //            (bool isSuccessful, string result) reply = default;
        //            try
        //            {
        //                var profile = await buser.GetDefaultAccount()?.GetCachedProfileCardAsync(_dynastio) ?? null;
        //                var profilea = profile is null ? null : new
        //                {
        //                    personal_chest_items = string.Join(", ", profile.Chest.Items.Select(a => a.ItemType.ToString() + " count " + a.Count)),
        //                    badges = string.Join(", ", profile.Profile.Badges.Select(a => a.ToString())),
        //                    coins = profile.Profile.Coins,
        //                    experience = profile.Profile.Experience,
        //                    lastestactivity = profile.Profile.LastActiveAt,
        //                    latestserver = profile.Profile.LatestServer,
        //                    level = profile.Profile.Level,
        //                };
        //                var systemPrompet = new
        //                {
        //                    SystemPrompet = SystemPrompet,
        //                    target_user_in_game_profile = JsonSerializer.Serialize(profilea),
        //                    target_user = JsonSerializer.Serialize(buser),
        //                    target_user_in_guild = JsonSerializer.Serialize(new
        //                    {
        //                        Id = target_user.Id,
        //                        Nickname = target_user.Nickname,
        //                        Username = target_user.Username,
        //                        JoinedAt = target_user.JoinedAt,
        //                        CreatedAt = target_user.CreatedAt,
        //                    }),
        //                };
        //                reply = await _ai.QueryAsync(null, JsonSerializer.Serialize(systemPrompet)).TryAsync();

        //            }
        //            catch
        //            {
        //                var systemPrompet = new
        //                {
        //                    SystemPrompet = SystemPrompet,
        //                };
        //                reply = await _ai.QueryAsync(null, JsonSerializer.Serialize(systemPrompet)).TryAsync();

        //            }
        //             await msg.ReplyAsync(reply.result);
        //            return true;
        //        }
    }
}