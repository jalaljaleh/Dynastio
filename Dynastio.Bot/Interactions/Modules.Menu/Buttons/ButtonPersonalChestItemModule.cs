using Discord;
using Discord.Interactions;
using Dynastio.Bot.Database;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Menu.Buttons
{
    /// <summary>
    /// TEMPLATE: Copy this class when you need to add a new button module.
    /// Acts as the “default” fallback for any unregistered or unknown button IDs.
    /// Inherit from MenuModulesBase and implement IButtonsServiceModule.
    /// </summary>
    public class ButtonPersonalChestItemModule : MenuModulesBase, IMenuComponentRule
    {
        // -----------------------------------------------------------------------------------
        // SECTION: Constants
        // -----------------------------------------------------------------------------------
        public DynastioApi Dynastio { get; set; }
        public DynastioItemsService ItemsService { get; set; }
        /// <summary>
        /// Prefix used on every custom ID for this module.
        /// Discord components with IDs starting with this value will be routed here.
        /// </summary>
        public const string InteractionIdBase = "interactions.menu.buttons.personalchest.item";

        /// <summary>
        /// Suffix format appended after the base ID.
        /// {0} = page, {1} = page size, {2} = trigger context.
        /// Allows you to pass parameters through the button’s CustomId.
        /// </summary>
        public const string IdParameterFormat = ":{0}:{1}:{2}";

        // -----------------------------------------------------------------------------------
        // SECTION: Builder Method
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Construct the ButtonBuilder that appears in the UI whenever this module is used.
        /// Copy-and-paste this method into your new module and adjust:
        /// - Label text
        /// - Emote key
        /// - Button style
        /// - CustomId construction
        /// </summary>
        /// <param name="args">
        /// Optional string parameters that will be embedded in the CustomId.
        /// Helps pass context (like page or filter) back to ExecuteAsync.
        /// </param>
        /// <returns>A fully configured ButtonBuilder instance.</returns>
        public static ButtonBuilder BuildButton(MenuModulesBase module, PersonalChestItem item, int accountIndex, params string[] args)
        {
            var btn = new ButtonBuilder()
                .WithLabel("Slot 0")
                .WithEmote(module.EmoteService.GetEmoteByName("unknown"))
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(item == null)
                .WithCustomId(BuildCustomId("unkown", accountIndex, trigger: CustomIdHelper.Generate()));

            if (item != null)
            {
                btn
                .WithLabel("Slot " + (item.Index + 1).ToRegularCounter())
                .WithEmote(module.EmoteService.GetEmote(item.ItemType))
                .WithCustomId(BuildCustomId(item.ItemType.ToString(), accountIndex, trigger: CustomIdHelper.Generate()));
            }

            return btn;
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Custom ID Factory
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Produces the CustomId string for this module’s button.
        /// Call this in your BuildButton override to ensure Consistent ID format.
        /// </summary>
        /// <param name="page">Page number (default = 1).</param>
        /// <param name="take">Items per page (default = 10).</param>
        /// <param name="trigger">Context label (default = empty).</param>
        /// <returns>Fully formatted CustomId for use with ComponentInteraction.</returns>
        public static string BuildCustomId(string item, int accountIndex, string trigger = "")
        {
            // Concatenate base prefix + formatted parameters
            // .StarIfNullFormat ensures safe formatting even if trigger is null/empty
            return InteractionIdBase
                 + IdParameterFormat.StarIfNullFormat(item, accountIndex, trigger);
        }

        // -----------------------------------------------------------------------------------
        // SECTION: Interaction Handler
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// This method is invoked when Discord receives a button click
        /// whose CustomId matches InteractionIdBase + IdParameterFormat.
        /// Copy-and-paste into your new module and adjust the attribute:
        /// [ComponentInteraction(YourBase + YourFormat)]
        /// </summary>
        [ComponentInteraction(InteractionIdBase + ":*:*:*")]
        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [RequireLinkedAccount]
        [RequireContext(ContextType.Guild)]
        public async Task ExecuteAsync(ItemType itemtype, int accountIndex, string trigger = "")
        {
            await DeferAsync();

            var account = accountIndex == 0 ? Context.BotUser.GetDefaultAccount() : Context.BotUser.Accounts[accountIndex - 1];
            var chest = await account.GetCachedPersonalChestAsync(Dynastio);
            var target = chest.Items.FirstOrDefault(a => a.ItemType == itemtype);

            string ownerMention = " Not Found ";
            if (!string.IsNullOrEmpty(target.OwnerId))
            {
                if (target.OwnerId.Contains("discord"))
                {
                    ownerMention = $"<@{target.OwnerId.Replace("discord:", "")}>";
                }
                else
                {
                    var itemOwner = await this.Context.Database.GetUserByConnectedAccountIdAsync(target.OwnerId);
                    if (itemOwner != null)
                    {
                        ownerMention = $"<@{itemOwner.Id}>";
                    }
                }
            }
            var item = ItemsService.GetItem(target.ItemType.ToString().ToLower());
            var sectionAccount = new SectionBuilder()
                           .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmote(itemtype).Url))
                        // .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("privatechest").Url))
                        .WithTextDisplay(
                          $"# {EmoteService.GetEmoteByName("zoom_in")} Slot {target.Index + 1}  ` {target.ItemType} `\n" +
                          $"### {EmoteService.GetEmoteByName("privatechest")} Personal Chest {account.DisplayName} \n" +
                          $"\n### Slot capacity: {EmoteService.BuildProgressBar(5, target.Count, item.Stack ?? 1)} **[{target.Count}/{item.Stack}]**" +
                          $"\n### Item Durability: {EmoteService.BuildProgressBar(10, target.Durability, item.Durability ?? 0)}" +
                          $"" +
                          $"\n");

            // Top‐level item properties
            var contentSectionTopLevelItemProperties =
                             $"Durability: `{NV(item.Durability)}`\n" +
                             $"Repair Price: `{NV(item.RepairPrice)}`\n" +
                             $"Weight: `{NV(item.Weight, d => d.ToString("0.##"))}`\n" +
                             $"Grade: `{NV(item.Grade)}`\n" +
                             $"Personal Only: {NV(item.Personal, b => b ? ":white_check_mark: " : ":x:")}\n";

            var sectionTopLevelItemProperties = new SectionBuilder()
               .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("polarbowrecipe").Url))
               .WithTextDisplay("## Item properties")
               .WithTextDisplay(contentSectionTopLevelItemProperties);


            // Items Action

            var a = item.ItemAction;
            var contentSectionItemActionProperties =
                        $"Type: ` {a.Type} `\n" +
                        $"Damage: ` {NV(a.Damage)} `\n" +
                        $"Power: ` {NV(a.Power)} `\n" +
                        $"Dodge: ` {NV(a.Dodge)} `\n" +
                        $"Distance: ` {NV(a.Distance)} `\n" +
                        $"Stamina: ` {NV(a.Stamina)} `\n" +
                        $"Bullet Speed: ` {NV(a.BulletSpeed)} `\n" +
                        $"Make Fire: {NV(a.MakeFire, b => b ? ":white_check_mark: " : ":x:")}\n" +
                        $"Redirect: {NV(a.Redirect, b => b ? ":white_check_mark: " : ":x:")}\n";


            var sectionItemActionProperties = new SectionBuilder()
              .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("archermannequin").Url))
              .WithTextDisplay("## Action properties")
              .WithTextDisplay(contentSectionItemActionProperties);

            var container = new ContainerBuilder()
              .WithAccentColor(Color.Green)
              .WithMediaGallery(AssetUrlService[AssetType.banner_dynastio])
              .WithTextDisplay($"You logined as {account.DisplayName} !")

              .WithSeparator(SeparatorSpacingSize.Large, true)
              .WithSection(sectionAccount)
                .WithSeparator(SeparatorSpacingSize.Small, true)
              .WithSection(sectionTopLevelItemProperties)
                .WithSeparator(SeparatorSpacingSize.Small, true)
              .WithSection(sectionItemActionProperties)
                .WithSeparator(SeparatorSpacingSize.Small, true)
              .WithTextDisplay(
                          $"\n### Details: ` {target.Details} `" +
                          $"\nOwner ` crafted by `: || {ownerMention} ||" +
                          $"\nToken: ` {target.Token} `" );

            //    .WithActionRow([ButtonCloseModule.BuildButton(this)]);

            var component = new ComponentBuilderV2()
                .WithContainer(container)
                ;
            await ReplyOrModifyAsync(components: component.Build());
        }

        // ------------------------------------------------------
        // Helper formatting methods
        // ------------------------------------------------------
        private static string NV<T>(T? value, Func<T, string>? format = null) where T : struct
        {
            if (value.HasValue)
                return format != null ? format(value.Value) : value.Value.ToString()!;
            return "—";
        }

    }
}
