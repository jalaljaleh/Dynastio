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

        public static ButtonBuilder GetTelegramButton()
        {
            return new ButtonBuilder()
            {
                Url = "https://t.me/halunteam/27",
                Label = "Report bugs | Сообщайте о багах",
                Style = ButtonStyle.Link,
            };
        }
        public static ButtonBuilder GetDiscordButton()
        {
            return new ButtonBuilder()
            {
                Url = "https://discord.gg/rbfnf9VZVZ",
                Label = "Disocrd Server",
                Style = ButtonStyle.Link,
            };
        }
        public async Task ReplyWithSuccessAsync(string message)
        {

            var header = new SectionBuilder()
                .WithTextDisplay($"# {EmoteService.GetEmote(Net.BadgeType.Friend)} All Set !")
                .WithTextDisplay($"{UserMention} Everything went through without a hitch. You’re good to go !")
                .WithAccessory(new ThumbnailBuilder(Context.User.TryGetAvatarUrl()));

            var containerb = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
                .WithAccentColor(Color.Green)
                .WithSection(header)
                
                .WithSeparator(SeparatorSpacingSize.Small,true)
                .WithTextDisplay($"{message}");

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb);

            cb.WithActionRow([GetDiscordButton(),GetTelegramButton()]);

            await ReplyOrModifyAsync(components: cb.Build());
        }
        public async Task ReplyWithErrorAsync(string message)
        {
            var containerb = new ContainerBuilder()
                .WithMediaGallery(AssetUrlService[AssetType.banner_error])
                .WithTextDisplay($"# {EmoteService.GetEmote(Net.BadgeType.Developer)}  Boar Gate Crash! !")
               .WithTextDisplay($"{EmoteService.GetEmote(Net.EntityType.Scooter)} {UserMention} Your command was ambushed by boar raiders at the data gate. Our pixelated knights are regrouping—try again in a moment.");
         
            var containerc = new ContainerBuilder()
                .WithAccentColor(Color.Red)
                .WithTextDisplay($"### Error Message:```{message}```");

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb)
                .WithContainer(containerc);


            cb.WithActionRow([GetDiscordButton(), GetTelegramButton()]);

            await ReplyOrModifyAsync(components: cb.Build());
        }

        public async Task ReplyWithNotFoundAsync()
        {
            //     var sb1 = new SectionBuilder()
            // .WithMediaGallery(AssetUrlService[AssetType.banner_not_found])

            var containerb = new ContainerBuilder()
                .WithMediaGallery(Common.Random.Next(1, 10) == 9 ? AssetUrlService[AssetType.banner_not_found_gif] : AssetUrlService[AssetType.banner_not_found])
                .WithAccentColor(Color.DarkerGrey)
                .WithTextDisplay($"# {EmoteService.GetEmoteByName("shadow1")}  The Nightmare’s Empty Feast !")
                .WithTextDisplay($"{EmoteService.GetEmote(Net.EntityType.Lamp)} {UserMention} By the light of a waning moon, the hungry Nightmare crept through these halls and swallowed every last record. What you see now is its aftermath: a silent void where data once danced.\nAdjust your filters, widen your search, and breathe fresh life into this page—before the Nightmare returns for another midnight banquet.")
                 ;

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb);


            cb.WithActionRow([GetDiscordButton(), GetTelegramButton()]);

            await ReplyOrModifyAsync(components: cb.Build());
        }
        public async Task<IUserMessage> ReplyOrModifyAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null,
            MessageFlags messageFlags = MessageFlags.None)
        {

            if (CurrentMessage == null)
            {
                if (this.Context.Interaction.HasResponded)
                {
                    var res = await FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed, flags: messageFlags);
                    return res;
                }
                else
                {
                    await RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, components, embed, flags: messageFlags);
                    return default;
                }
            }

            await CurrentMessage.ModifyAsync(x =>
             {
                 x.Content = text;
                 x.AllowedMentions = allowedMentions;
                 x.Attachments = null;
                 x.Components = components;
                 x.Embed = embed;
                 x.Embeds = embeds;
                 x.Flags = messageFlags == MessageFlags.None ? CurrentMessage.Flags.Value : messageFlags;
             });
            return CurrentMessage;
        }
    }
}