using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Dynastio.Bot.Extensions;
using Newtonsoft.Json;
using Dynastio.Net;
using Dynastio.Data;

namespace Dynastio.Bot.Interactions
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly Configuration _configuration;
        private readonly UserService userService;
        private readonly LocaleService localeService;
        private readonly IDatabaseContext _db;
        private readonly GuildService _guildservice;

        public DynastioClient _dynastClient { get; set; }
        public InteractionHandler(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _handler = services.GetRequiredService<InteractionService>();
            _configuration = services.GetRequiredService<Configuration>();
            userService = services.GetRequiredService<UserService>();
            _dynastClient = services.GetRequiredService<DynastioClient>();
            localeService = services.GetRequiredService<LocaleService>();
            _guildservice = services.GetRequiredService<GuildService>();
            _db = services.GetRequiredService<IDatabaseContext>();
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Process when the client is ready, so we can register our commands.
            _client.Ready += ReadyAsync;
            _handler.Log += LogAsync;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            _handler.InteractionExecuted += _handler_InteractionExecuted;
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
            if (Program.IsDebug())
                await _handler.RegisterCommandsToGuildAsync(_configuration.Guilds.DebugServer, true);
            else
                await _handler.RegisterCommandsGloballyAsync(true);


            //var cmds = await _client.GetGlobalApplicationCommandsAsync();
            //foreach (var cmd in cmds)
            //    await cmd.DeleteAsync();

        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            if (!InteractionUtilities.IsStaticInteractionCommand(interaction)) return;

            var user = await userService.GetUserAsync(interaction.User.Id);
            
            if (user.IsBanned)
            {
                await interaction.RespondAsync(ephemeral: true, embed: "Unfortunately, your access to the bot is **limited**, you cannot use the bot.".ToDangerEmbed());
                return;
            }

            var locale = localeService[interaction.UserLocale];
            var guild = await _guildservice.GetGuildAsync(interaction.GuildId.Value);

            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new CustomSocketInteractionContext(_client, interaction, user, locale, guild);

            // Execute the incoming command.
            var result = await _handler.ExecuteCommandAsync(context, _services);
        }
        public async Task LogErrorAsync(ICommandInfo commandInfo, IInteractionContext context_, IResult result)
        {
            //if (Program.IsDebug())
            //    return;

            if (result.ErrorReason == "Input string was not in a correct format.")
                return;

            var content = JsonConvert.SerializeObject(result) +
                          $"\nName: {commandInfo.Name}" +
                          $"\nMethodName: {commandInfo.MethodName}" +
                          $"\nUser: {context_.User.Id}";

            var channel = _client.GetGuild(_configuration.Guilds.MainServer)
                .GetTextChannel(_configuration.Channels.ErrorLoggerChannel);

            await DiscordStream.SendStringAsFile(channel, content);
        }
        private async Task _handler_InteractionExecuted(ICommandInfo commandInfo, IInteractionContext context_, IResult result)
        {
            if (result.IsSuccess) return;

            var context = context_ as ICustomInteractionContext;
            if (result.Error == InteractionCommandError.UnmetPrecondition && result.ErrorReason == "RequireUser")
            {
                await context_.Interaction.RespondAsync(context.Locale["access_denied.description"], ephemeral: true);
                return;
            }
            if (result.Error == InteractionCommandError.Exception && result.ErrorReason == "Cannot respond to an interaction after 3 seconds!")
            {
                return;
            }
            if (result.Error == InteractionCommandError.Exception && result.ErrorReason == "The server responded with error 50013: Missing Permissions")
            {
                await context_.Interaction.User.SendMessageAsync("The server responded with error 50013: Missing Permissions, the bot have a missing permission");
                return;
            }

            string Title = "";
            string Description = "";

            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    Title = context.Locale["unmet_precondition"];
                    Description = $"\n{context.Locale["you_have_unmet_precondition"]}:\n" + result.ErrorReason;
                    break;
                case InteractionCommandError.Unsuccessful:
                    Title = "Unsuccessful";
                    Description = $"Reason: No Reason.";
                    break;
                case InteractionCommandError.ParseFailed:
                    Title = "Parse Failed";
                    Description = $"Reason: " + result.ErrorReason;
                    break;
                case InteractionCommandError.ConvertFailed:
                    Title = "Convert Failed";
                    Description = $"Reason: " + result.ErrorReason;
                    break;
                case InteractionCommandError.BadArgs:
                    Title = "Bad Args";
                    Description = $"Reason: " + result.ErrorReason;
                    break;
                case InteractionCommandError.UnknownCommand:
                    Title = "Command Not Found";
                    Description = $"Reason: " + result.ErrorReason;
                    break;
                case InteractionCommandError.Exception:
                    Title = context.Locale["unknown_error"];
                    Description = $"Reason: Unknown Error, this bug reported to the bot developer.";
                    await LogErrorAsync(commandInfo, context_, result).Try();
                    break;
                default:
                    Title = context.Locale["unknown_error"];
                    Description = $"Reason: Unknown Error";
                    break;
            }
            if (context_.Interaction.Type is InteractionType.ApplicationCommand)
            {
                if (!context.Interaction.HasResponded)
                    await context.Interaction.DeferAsync(true);

                await context.Interaction.FollowupAsync(embed: Description.ToWarnEmbed(Title), ephemeral: true);
                return;
            }
            if (context_.Interaction.Type is InteractionType.MessageComponent)
            {
                if (commandInfo.Attributes.OfType<UnmodifiableContentAttribute>().FirstOrDefault() != null)
                {
                    if (!context.Interaction.HasResponded)
                        await context.Interaction.DeferAsync(true);

                    await context.Interaction.FollowupAsync(Title + "\n" + Description, ephemeral: true);
                    return;
                }
                await (context_.Interaction as IComponentInteraction).Message.ModifyAsync((x) =>
                {
                    x.Content = context_.User.Id.ToUserMention();
                    x.Embed = Description.ToDangerEmbed(Title);
                    x.Embeds = new Embed[] { };
                    x.Attachments = new Optional<IEnumerable<FileAttachment>>();
                    x.Components = new ComponentBuilder().Build();
                });
                return;
            }

        }

    }
}

