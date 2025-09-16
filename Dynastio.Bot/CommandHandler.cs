using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Bot.Commands;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Google.Apis.Services;
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
        private readonly ClientService _clientService;
        private readonly IServiceProvider _services;
        public CommandHandlerService(IServiceProvider services)
        {
            _services = services;
            _dynastio = services.GetRequiredService<DynastioApi>();
            _commands = services.GetRequiredService<CommandService>();
            _clientService = services.GetRequiredService<ClientService>();
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

        public async Task<IResult> ExecuteCommandAsync(SocketUserMessage message, Guild guild, User user)
        {
            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return null;

            var context = new BotSocketCommandContext(_services, _discord, message, user, guild);
            return await _commands.ExecuteAsync(context, argPos, _services);
        }

        private const ulong DevUserId = 1374305522290917526;
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext _context, IResult result)
        {
            var context = _context as BotSocketCommandContext;
            if (!command.IsSpecified)
            {
                await SpecifiedCommandAsync(command, context);
                return;
            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            // await context.Channel.SendMessageAsync($"error: {result}");
        }
        public async Task SpecifiedCommandAsync(Optional<CommandInfo> command, BotSocketCommandContext context)
        {
            if (context.BotGuild.Id != 480416088312774657)
                return;

            if (!_ai.TryAcquireSlot())
            {
                await context.Message.AddReactionAsync(new Emoji("😂")).ConfigureAwait(false);
                return;
            }
            await _ai.ReplyMessageAsync(context.Message, context.BotUser).ConfigureAwait(false);
        }


    }
}