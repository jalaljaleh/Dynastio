namespace Discord.Interactions
{
    using Discord.Interactions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dynastio.Bot;
    using Discord;
    public class RequireConfirmation : PreconditionAttribute
    {
        public RequireConfirmation(
            string title = "Confirmation Form",
            string description = "Are you sure about executing the command, The action may not be undone.",
            int timeout = 20,
            string confirmLabel = "Confrim",
            string cancelLabel = "Cancel")
        {
            this.description = description;
            this.title = title;
            this.timeout = timeout;
            this.btnCancelLabel = cancelLabel;
            this.btnConfirmLabel = confirmLabel;
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

            var embed = description.ToWarnEmbed(title);

            var component = new ComponentBuilder()
                .WithButton(btnConfirmLabel, $"{InteractionUtilities.Perfix}confirm", ButtonStyle.Success)
                .WithButton(btnCancelLabel, $"{InteractionUtilities.Perfix}cancel", ButtonStyle.Danger)
                .Build();

            var msg = await context.Interaction.FollowupAsync(embed: embed, components: component);
            var result = await DiscordInput.ReadButtonFromMessageAsync(context, msg, TimeSpan.FromSeconds(timeout), true, true, true);

            await msg.DeleteAsync();

            if (result == null)
                return PreconditionResult.FromError(this.ErrorMessage);


            if (result.Data.CustomId == $"{InteractionUtilities.Perfix}confirm")
            {
                (context as ICustomInteractionContext).OverridenInteraction = result;
                return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError(this.ErrorMessage);
        }
    }
}
