/*!
 * Discord Locale v1.1 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/Template/Interactions/Precondinations/RequireConfirmation.cs)
 */
namespace Dynastio.Bot.Interactions
{
    using Discord;
    using Discord.Interactions;
    using Dynastio.Bot.Addons;
    using System;
    using System.Threading.Tasks;


    public class RequireConfirmation : PreconditionAttribute
    {
        public RequireConfirmation(
            string title = "Confirmation",
            string description = "Are you sure about executing the command, The action may not be undone.",
            int timeout = 20,
            string confirmLabel = "Confrim",
            string cancelLabel = "Cancel")
        {
            this.description = description;
            this.title = title;
            this.timeout = timeout;
            btnCancelLabel = cancelLabel;
            btnConfirmLabel = confirmLabel;
        }
        private string description = "";
        private string title = "";
        private int timeout = 20;
        private string btnConfirmLabel;
        private string btnCancelLabel;

        public new string ErrorMessage = $"This command require confirmation.";
        public async override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            await context.Interaction.DeferAsync();

            var embed = new EmbedBuilder()
            {
                Title = title,
                Description = description,
                Color = Color.Orange
            }.Build();

            var component = new ComponentBuilder()
                .WithButton(btnConfirmLabel, DiscordInput.GenerateCustomId("confirm"), ButtonStyle.Success)
                .WithButton(btnCancelLabel, DiscordInput.GenerateCustomId("cancel"), ButtonStyle.Danger)
                .Build();

            var message = await context.Interaction.FollowupAsync(embed: embed, components: component);
            var interactionResult = await DiscordInput.WaitForButtonFromMessageAsync(context, message, TimeSpan.FromSeconds(timeout), true, true, true);

            await message.DeleteAsync();

            if (interactionResult == null || interactionResult.Data.CustomId != DiscordInput.GetCustomId("confirm"))
                return PreconditionResult.FromError(ErrorMessage);

            (context as BotSocketInteractionContext).OverridedInteraction = interactionResult;
            return PreconditionResult.FromSuccess();
        }
    }
}