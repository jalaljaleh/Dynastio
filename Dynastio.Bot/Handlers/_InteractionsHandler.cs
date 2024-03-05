using Discord.Interactions;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Global;
using Dynastio.Bot.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal class InteractionsHandler : HandlersBase
    {
        private readonly InteractionService _interactions;

        public InteractionsHandler(IServiceProvider services) : base(services)
        {
            _interactions = services.GetRequiredService<InteractionService>();
        }
        public async Task InitializeAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

            _discord.Ready += _discord_Ready;

            _discord.InteractionCreated += _discord_InteractionCreated;
            _interactions.InteractionExecuted += _interactions_InteractionExecuted;
        }

        private async Task _discord_Ready()
        {
            if (Main.IsDebug())
                await _interactions.RegisterCommandsToGuildAsync(_config.DebugServerId, true);
            else
                await _interactions.RegisterCommandsGloballyAsync(true);
        }

        private List<ulong> users = new List<ulong>();
        private async Task _discord_InteractionCreated(Discord.WebSocket.SocketInteraction interaction)
        {
            if (interaction.Type is Discord.InteractionType.MessageComponent or Discord.InteractionType.ModalSubmit)
            {
                if (DiscordInput.IsFromDiscordInput(interaction)) return;
            }

            if (users.Contains(interaction.User.Id))
            {
                await interaction.RespondAsync(embed: "Another interaction is running.".ToEmbed("Wait .."));
                return;
            }
            users.Add(interaction.User.Id);

            var ctx = new BotSocketInteractionContext(_discord, interaction, _services);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }
        private async Task _interactions_InteractionExecuted(ICommandInfo info, Discord.IInteractionContext context, IResult result)
        {
            users.Remove(context.User.Id);

            if (result.IsSuccess)
                return;

            if (context.Interaction.HasResponded)
                await context.Interaction.FollowupAsync(result.ErrorReason, ephemeral: true);
            else
                await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
        }


    }
}
