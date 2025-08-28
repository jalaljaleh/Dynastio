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
        public async Task ModifyCurrentMessageToNotFound()
        {
            //     var sb1 = new SectionBuilder()
            // .WithMediaGallery(AssetUrlService[AssetType.banner_not_found])

            var containerb = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_not_found])
                .WithAccentColor(Color.DarkerGrey)
                .WithTextDisplay($"# {EmoteService.GetEmoteByName("shadow1")}  The Nightmare’s Empty Feast !")
                .WithTextDisplay($"{EmoteService.GetEmote(Net.EntityType.Lamp)} {UserMention} By the light of a waning moon, the hungry Nightmare crept through these halls and swallowed every last record. What you see now is its aftermath: a silent void where data once danced.\nAdjust your filters, widen your search, and breathe fresh life into this page—before the Nightmare returns for another midnight banquet.")
                 ;

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