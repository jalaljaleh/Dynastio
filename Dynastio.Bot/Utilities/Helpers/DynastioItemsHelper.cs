using Dynastio.Net;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Utilities.Helpers
{
    public static class DynastioItemsCategory
    {
        // raw materials, crafting ingredients, stackables
        public static readonly ItemType[] Resources = new[]
        {
            ItemType.Meat, ItemType.Candy, ItemType.IceBall, ItemType.Snowball,
        ItemType.Log, ItemType.DeadLog, ItemType.Rock, ItemType.GoldOre,
        ItemType.CopperOre, ItemType.TinOre, ItemType.TimeriteOre, ItemType.Diamond,
        ItemType.Powder, ItemType.Bone, ItemType.Dust, ItemType.Chitin,
        ItemType.Fiber, ItemType.GhostThread, ItemType.CrystalThreads,
        ItemType.MagickDust, ItemType.Feather, ItemType.Web, ItemType.Coin,
        ItemType.PolarCoin, ItemType.HalloweenCoin, ItemType.WebBall
    };

        // melee, ranged, magical & special attack items
        public static readonly ItemType[] Weapons = new[]
        {
        // swords & blades
        ItemType.Sword, ItemType.GoldenSword, ItemType.DarkSword,
        ItemType.Katana, ItemType.GoldenKatana, ItemType.RubySword,
        ItemType.LaserSword, ItemType.AstraSword,

        // polearms & spears
        ItemType.Spear, ItemType.DarkSpear, ItemType.TimeriteSpear,
        ItemType.CopperSpear, ItemType.BronzeSpear, ItemType.AstraSpear,

        // bows & arrows
        ItemType.Bow, ItemType.GoldenBow, ItemType.PremiumBow,
        ItemType.ChitinBow, ItemType.PhantomBow,
        ItemType.Arrow, ItemType.PhantomArrow, ItemType.Harpoon,

        // thrown weapons
        ItemType.ThrowingKnife, ItemType.DarkThrowingKnife,
        ItemType.Suriken, ItemType.NightmareSuriken, ItemType.Boomerang,
        ItemType.DarkBoomerang, ItemType.CrowBar, ItemType.GrapplingHook,

        // magical wands & orbs
        ItemType.MagicWand, ItemType.HealWand, ItemType.Thunderwand,
        ItemType.AcidWand, ItemType.SpiderWand, ItemType.IceWand,
        ItemType.LightWand, ItemType.StaffOfPlague,

        ItemType.Fireball,
        ItemType.MagicBall,
        ItemType.PolarBombPotion, ItemType.SlimeWeapon, // special cases
        ItemType.Bandage, ItemType.GatherPotion, ItemType.BuildingPotion,
        ItemType.DropPotion, ItemType.ExperiencePotion, ItemType.FirePotion,


        ItemType.GrenadeSmoke, ItemType.GrenadeFire, ItemType.GrenadeIce,
        ItemType.GrenadeToxic, ItemType.GrenadeSticky,

        ItemType.BronzeCrate, ItemType.SilverCrate, ItemType.GoldenCrate,
        ItemType.HolidayCrate, ItemType.BronzeKey, ItemType.SilverKey,
        ItemType.GoldenKey, ItemType.Recipe, ItemType.PolarSwordRecipe,
        ItemType.CactusSwordRecipe, ItemType.FlowerCactusHatRecipe,


        ItemType.SimpleShield, ItemType.GoldenShield, ItemType.DarkShield,
        ItemType.SpikeShield, ItemType.DiamondShield, ItemType.ReflectShield,
        ItemType.DeathShield, ItemType.PhantomShield, ItemType.ChitinShield,
        ItemType.TimeriteShield, ItemType.IceShield, ItemType.HalloweenShield
    };

        // headgear with passive or cosmetic effects
        public static readonly ItemType[] Hats = new[]
        {
        ItemType.PropellerHat, ItemType.BushHat, ItemType.SpeedHat,
        ItemType.LampHat, ItemType.NightmareHat, ItemType.FurHat,
        ItemType.BoneHat, ItemType.SpikeHat, ItemType.CrownHat,
        ItemType.DruidHat, ItemType.VampireHat, ItemType.BearHat,
        ItemType.TechHat, ItemType.TouristHat, ItemType.ThiefHat,
        ItemType.JasonHat, ItemType.SantaHat, ItemType.HarlequinHat,
        ItemType.MagicHat, ItemType.PinkShooterHat, ItemType.CowHat,
        ItemType.EagleHat, ItemType.GoldenHelmet,
        ItemType.SpiderHat, ItemType.CactusHat, ItemType.MedicineHat,
        ItemType.PolarHat, ItemType.PumpkinHat
    };



        // crafting-oriented tools & utilities
        public static readonly ItemType[] Utilities = new[]
        {
              // axes & clubs
        ItemType.Axe, ItemType.GoldenAxe, ItemType.VikingAxe,
        ItemType.GoldenVikingAxe, ItemType.Boneclub, ItemType.Club,
        ItemType.Hammer, ItemType.GoldenHammer, ItemType.WoodenHammer,
        ItemType.IronClub, ItemType.IceSpear, ItemType.Machete,
        ItemType.BatWithNails, ItemType.HappyClub, ItemType.Pickaxe,
        ItemType.GoldenPickaxe, ItemType.DiamondPickaxe,

        ItemType.Tool, ItemType.Extinguisher, ItemType.GoldenPickaxe,
        ItemType.CopperAxe, ItemType.BronzePickaxe, ItemType.GrapplingHook,
        ItemType.Scissors, ItemType.Lollipop, ItemType.CharmOfReturn,
        ItemType.PremiumCharmOfReturn, ItemType.TimeriteBlock, ItemType.Nails,

    };



        // “skin switcher” cosmetics
        public static readonly ItemType[] CosmeticSkins = new[]
        {
        ItemType.SkinSwitcher_PremiumDarkKnight, ItemType.SkinSwitcher_Halloween,
        ItemType.SkinSwitcher_Anime, ItemType.SkinSwitcher_Robot,
        ItemType.SkinSwitcher_Soldier, ItemType.SkinSwitcher_SwampPioneer,
        ItemType.SkinSwitcher_VoidKnight, ItemType.SkinSwitcher_Shrek,
        ItemType.SkinSwitcher_PlagueDoctor, ItemType.SkinSwitcher_Daemon,
        ItemType.SkinSwitcher_Random
    };
    }
}
