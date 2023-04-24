using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly LocaleService localeService;
        private readonly GuildService _guildservice;
        private readonly UserService _userService;
        private readonly DynastioClient _dynastioclient;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _guildservice = services.GetRequiredService<GuildService>();
            localeService = services.GetRequiredService<LocaleService>();
            _userService = services.GetRequiredService<UserService>();
            _dynastioclient = services.GetRequiredService<DynastioClient>();
            _services = services;

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            // Ignore direct messages
            if (!(message.Channel is IGuildChannel guildChannel)) return;

            var guild = await _guildservice.GetGuildAsync(guildChannel.GuildId);
            var locale = localeService[guildChannel.Guild.PreferredLocale] ?? localeService["en-US"];

            bool moderaotr = false;
            if (guild.IsModerationEnabled)
            {
                moderaotr = await ChannelUtilities.ExecuteImageOnlyModule(guild, locale, guildChannel.Id, message);
                if (moderaotr) return;
            }

            var argPos = 0;
            if (!message.HasStringPrefix(".", ref argPos)) return;

            var user = await _userService.GetUserAsync(message.Author.Id);

            var context = new CustomSocketCommandContext(_discord, message, guild, user, locale);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            if (result.Error == CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                return;
            }
            await context.Channel.SendMessageAsync($"error");

        }
    }
}
