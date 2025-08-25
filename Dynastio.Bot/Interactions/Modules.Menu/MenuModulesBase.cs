using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extensions;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally.Extensions;
using Dynastio.Bot.Utilities;

namespace Dynastio.Bot.Interactions
{
    //
    // Summary:
    //     Provides a base class for Menu modules to inherit from.
    //
    public class MenuModulesBase : BotInteractionModuleBase<BotSocketInteractionContext>
    {
        public async Task CloseMenuAsync()
        {
            var embed = new EmbedBuilder()
            {
                Title = this["menu.closed.title"],
                Description = this["menu.closed.description"] + "\n\n",
                ThumbnailUrl = BotAvatarUrl,
                Color = Color.Orange,
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder().WithIsInline(true)
                    .WithName("Waiting time")
                    .WithValue("Since " +DateTime.UtcNow.ToDiscordTimestamp())
                },
            }.Build();

            await ModifyMenuMessageAsync(UserMention, embed: embed, components: new ComponentBuilder().Build());
        }
        public async Task NotFound()
        {
            var sb1 = new SectionBuilder()
                .WithTextDisplay($"## {EmoteService.GetEmoteByName("unknown")}  Not Found !")
                .WithTextDisplay($"{EmoteService.GetEmote(Net.EntityType.Lamp)} Hey {UserMention} The land is quiet, no matching survivors are found. !")
                .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmote(Net.BadgeType.MapMaker).Url, "Dynast.io Bot", false));

            var containerb = new ContainerBuilder()
                .WithAccentColor(Color.Orange)
                .WithSection(sb1);

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb);

            await ModifyMenuMessageAsync(components: cb.Build());
        }
        public async Task<IUserMessage> ModifyMenuMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
        {
            await CurrentMessage.ModifyAsync(x =>
             {
                 x.Content = text;
                 x.AllowedMentions = allowedMentions;
                 x.Attachments = null;
                 x.Components = components;
                 x.Embed = embed;
                 x.Embeds = embeds;
                 x.Flags = MessageFlags.ComponentsV2;
             });
            return CurrentMessage;
        }
    }
}