using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Dynastio.Bot
{
    public class InteractionsHandler
    {
        private readonly IServiceProvider _services;
        private readonly InteractionService _interactions;
        private readonly DiscordSocketClient _client;
        private readonly Configuration _config;

        public InteractionsHandler(IServiceProvider services)
        {
            this._services = services;
            _interactions = services.GetRequiredService<InteractionService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<Configuration>();
        }
        public async Task InitializeAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

            _client.Ready += _client_Ready;
            async Task _client_Ready()
            {
                    await _interactions.RegisterCommandsGloballyAsync(true);
            }

            _client.InteractionCreated += _client_IntegrationCreated;
            _interactions.InteractionExecuted += _interactions_InteractionExecuted;
        }

        private async Task _client_IntegrationCreated(SocketInteraction interaction)
        {
            if (DiscordInput.IsFromDiscordInput(interaction)) return;

            var ctx = new CustomSocketInteractionContext(_client, interaction, _services);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }


        private async Task _interactions_InteractionExecuted(ICommandInfo info, Discord.IInteractionContext context_, IResult result)
        {
            if (result.IsSuccess)
                return;

            if (context_.Interaction.HasResponded)
                await context_.Interaction.FollowupAsync(result.ErrorReason, ephemeral: true);
            else
                await context_.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);

        }
    }
}