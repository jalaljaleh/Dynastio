using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;


namespace Dynastio.Bot
{
    public class CommandsHandler
    {
        private readonly IServiceProvider _services;
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discord;
        private readonly Configuration _config;
        public CommandsHandler(IServiceProvider services)
        {
            _services = services;
            _commandService = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<Configuration>();

            _commandService.CommandExecuted += _commandService_CommandExecuted;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(typeof(CommandsHandler).Assembly, _services);
        }
        public async Task<IResult> HandleCommand(SocketUserMessage message)
        {
            if (message is null || _config is null || _config.Prefix is null) return null;

            var argPos = 0;
            if (!message.HasStringPrefix(_config.Prefix, ref argPos))
                return null;

            var context = new CustomCommandContext(_discord, message);

            return await _commandService.ExecuteAsync(context, argPos, _services);
        }
        private async Task _commandService_CommandExecuted(Discord.Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (result.IsSuccess) return;

            //if command not found
            if (!info.IsSpecified) return;

            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
