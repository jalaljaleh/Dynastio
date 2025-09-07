using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Net;
using Microsoft.Extensions.Logging.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Owner
{
    [RequireContext(ContextType.Guild)]
    public class ItemsModule : MenuModulesBase
    {
        public DynastioItemsService ItemsService { get; set; }

        public const string InteractionIdBase = "interactions.buttons.items.random";
        public const string IdParameterFormat = ":{0}";

        public static ButtonBuilder BuildButton(MenuModulesBase module, params string[] args)
        {
            var btn = new ButtonBuilder()
                .WithLabel("Random Item")
                .WithEmote(module.EmoteService.GetEmoteByName("touristhat"))
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(false)
                .WithCustomId(BuildCustomId(trigger: CustomIdHelper.Generate()));
            return btn;
        }
        public static string BuildCustomId(string trigger = "")
        {
            // Concatenate base prefix + formatted parameters
            // .StarIfNullFormat ensures safe formatting even if trigger is null/empty
            return InteractionIdBase
                 + IdParameterFormat.StarIfNullFormat(trigger);
        }

        [RequireMessageComponentTimeout]
        [RequireMessageComponentOwner]
        [ComponentInteraction(InteractionIdBase + ":*")]
        public async Task randomItemAsync(string trigger = "default")
        {
            await DeferAsync();
            await ModifyMenuMessageAsync(components: getContent(ItemsService.GetRandomItem()).Build());
        }

        [SlashCommand("items", "description")]
        public async Task items(string item)
        {
            await DeferAsync();

            var result = ItemsService.TryGetItem(item, out var target);
            if (result != false)
                await FollowupAsync(components: getContent(target).Build());
            else
                await FollowupAsync($"Can't find  {item}  !");
        }

        public ComponentBuilderV2 getContent(DynastioItem item)
        {
            var emote = EmoteService.GetEmoteByName(item.ItemType);


            var sectionHeader = new SectionBuilder()
                .WithAccessory(new ThumbnailBuilder(emote.Url))
                .WithTextDisplay($"# {item.ItemType}" +
                $"\nThis section breaks down exactly how the item behaves when used in the game. You’ll see its combat stats, usage timings, movement effects, projectile details, and any special conditions it applies. It also lists interaction tags, gathering capabilities, and armor values if applicable. Even if a value isn’t set for this item, it’s shown here so you can compare it with others at a glance.");

            // Top‐level item properties
            var contentSectionTopLevelItemProperties =
                             $"Item Type      : {NV(item.ItemType)}\n" +
                             $"Durability     : {NV(item.Durability)}\n" +
                             $"Grade          : {NV(item.Grade)}\n" +
                             $"Is Tool        : {NV(item.IsTool, b => b ? "Yes" : "No")}\n" +
                             $"Is Potion      : {NV(item.IsPotion, b => b ? "Yes" : "No")}\n" +
                             $"Personal Only  : {NV(item.Personal, b => b ? "Yes" : "No")}\n" +
                             $"Repair Price   : {NV(item.RepairPrice)}\n" +
                             $"Stack Size     : {NV(item.Stack)}\n" +
                             $"Weight         : {NV(item.Weight, d => d.ToString("0.##"))}\n" +
                             $"Custom Effect  : {NV(item.CustomEffectType)}\n" +
                             $"Tag Multipliers: {FormatDict(item.TagMul)}\n";

            var sectionTopLevelItemProperties = new SectionBuilder()
               .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("left_build_icon1").Url))
               .WithTextDisplay("## Item properties")
               .WithTextDisplay(contentSectionTopLevelItemProperties.ToCodeBlock("js"));


            // Items Action

            var a = item.ItemAction;
            var contentSectionItemActionProperties =
                        $"Type                   : {NV(a.Type)}\n" +
                        $"Entity Type            : {NV(a.EntityType)}\n" +
                        $"Attack Angle           : {NV(a.AttackAngle, d => d.ToString("0.##"))}\n" +
                        $"Make Fire              : {NV(a.MakeFire, b => b ? "Yes" : "No")}\n" +
                        $"Player                 : {NV(a.Player)}\n" +
                        $"Push                   : {NV(a.Push, d => d.ToString("0.##"))}\n" +
                        $"Damage                 : {NV(a.Damage, d => d.ToString("0.##"))}\n" +
                        $"Distance               : {NV(a.Distance, d => d.ToString("0.##"))}\n" +
                        $"Power                  : {NV(a.Power, d => d.ToString("0.##"))}\n" +
                        $"Dodge                  : {NV(a.Dodge, d => d.ToString("0.##"))}\n" +
                        $"Hand Type              : {NV(a.HandType)}\n" +
                        $"AI Distance Mul        : {NV(a.AiDistanceMul, d => d.ToString("0.##"))}\n" +
                        $"AI Use Predicted       : {NV(a.AiUsePredictedTransform, b => b ? "Yes" : "No")}\n" +
                        $"Stamina                : {NV(a.Stamina, d => d.ToString("0.##"))}\n" +
                        $"Stamina Pulse Timeout  : {NV(a.StaminaPulseTimeout, d => d.ToString("0.##"))}\n" +
                        $"Stamina Pulse Value    : {NV(a.StaminaPulseValue, d => d.ToString("0.##"))}\n" +
                        $"Bullet Entity Type     : {NV(a.BulletEntityType)}\n" +
                        $"Bullet Item Type       : {NV(a.BulletItemType)}\n" +
                        $"Bullet Offset          : {NV(a.BulletOffset, d => d.ToString("0.##"))}\n" +
                        $"Bullet Speed           : {NV(a.BulletSpeed, d => d.ToString("0.##"))}\n" +
                        $"Pre Timeout            : {NV(a.PreTimeout, d => d.ToString("0.##"))}\n" +
                        $"Post Timeout           : {NV(a.PostTimeout, d => d.ToString("0.##"))}\n" +
                        $"Ignore Shield          : {NV(a.IgnoreShield, b => b ? "Yes" : "No")}\n" +
                        $"Ignore Shield Chance   : {NV(a.IgnoreShieldChance, d => d.ToString("0.##"))}\n" +
                        $"Tag                    : {NV(a.Tag)}\n" +
                        $"Redirect               : {NV(a.Redirect, b => b ? "Yes" : "No")}\n" +
                        $"Allow Redirect         : {NV(a.AllowRedirect, b => b ? "Yes" : "No")}\n" +
                        $"Offset                 : {NV(a.Offset, d => d.ToString("0.##"))}\n" +
                        $"Animation Length       : {NV(a.AnimationLength, d => d.ToString("0.##"))}\n" +
                        $"Key Type               : {NV(a.KeyType)}\n";

            var sectionItemActionProperties = new SectionBuilder()
               .WithAccessory(new ThumbnailBuilder(EmoteService.GetEmoteByName("left_craft_icon").Url))
               .WithTextDisplay("## Action properties")
               .WithTextDisplay(contentSectionItemActionProperties.ToCodeBlock("js"));

            ContainerBuilder c1 = new ContainerBuilder()
                .WithAccentColor(Color.Default)
                .WithSection(sectionHeader)
                .WithSection(sectionTopLevelItemProperties)
                .WithSection(sectionItemActionProperties);


            ComponentBuilderV2 cb = new ComponentBuilderV2();
            cb.WithContainer(c1)
                .WithActionRow([BuildButton(this)]);

            return cb;
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
        private static string NV(string? value)
            => string.IsNullOrWhiteSpace(value) ? "—" : value!;
        private static string FormatList(IEnumerable<string>? list)
        {
            if (list == null)
                return "—";
            var items = new List<string>();
            foreach (var s in list)
                if (!string.IsNullOrWhiteSpace(s))
                    items.Add(s);
            return items.Count > 0 ? string.Join(", ", items) : "—";
        }
        private static string FormatDict(Dictionary<string, double>? dict)
        {
            if (dict == null || dict.Count == 0)
                return "—";
            var parts = new List<string>();
            foreach (var kv in dict)
                parts.Add($"{kv.Key}:{kv.Value:0.##}");
            return string.Join(", ", parts);
        }
    }

}
