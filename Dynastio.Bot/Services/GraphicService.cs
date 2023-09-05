using Dynastio.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Dynastio.Bot.Managers;
using Dynastio.Bot.Global;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Dynastio.Bot.Data;
using Discord;
using Image = SixLabors.ImageSharp.Image;
using Color = SixLabors.ImageSharp.Color;

namespace Dynastio.Bot
{
    public class GraphicService
    {
        private readonly InternetService _internetService;

        private readonly IServiceProvider _services;
        public GraphicService(IServiceProvider services)
        {
            _services = services;
            _internetService = _services.GetRequiredService<InternetService>();
        }
        public GraphicService Initialize()
        {
            Main.Log("Graphic Service", "Initializing ..");

            FontCollection collection = new FontCollection();
            fontFamily = collection.Add(FileManager.ToResourcePath("Fonts/FiraSans-Bold.ttf"));
            font = fontFamily.CreateFont(12, FontStyle.Bold);

            Main.Log("Graphic Service", "Initializied");

            return this;
        }
        public Font font { get; set; }
        public FontFamily fontFamily { get; set; }
        public async Task<Image> GetWelcomeImage(Discord.IGuildUser user)
        {
            Image image = Image.Load(FileManager.ToResourcePath($@"Images/welcome.jpg"));

            using (Image avatar = await _internetService.GetImageAsync(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()))
            {
                avatar.Mutate(wIndex => wIndex.Resize(238, 238));
                image.Mutate(wIndex => wIndex.DrawImage(avatar, new Point(30, 30), 1f));

                var users = await user.Guild.GetUsersAsync();

                image.Mutate(wIndex => wIndex.DrawText("Username: " + user.Username, new Font(fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 300)));
                image.Mutate(wIndex => wIndex.DrawText("Member: " + (users.Count + 1) + "th", new Font(fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 370)));
                image.Mutate(wIndex => wIndex.DrawText("Welcome to Dynast.io", new Font(fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 430)));
            }

            return image;
        }
     

        public enum ChestStyle
        {
            Default,
            Orange,
            Green
        }
        private const int wSlotSize = 6;
        private const int hSlotSize = 5;
        private const int SlotSizeWidth = 73;
        private const int SlotSizeHeight = 87;
        private const int firstSlotSizeMarginWidth = 288;
        private const int firstSlotSizeMarginHeight = 52;
        private const int SlotSizeMarginWidth = 5;
        private const int SlotSizeMarginHeight = 7;
        public async Task<Image> GetPersonalChestAsync(Discord.IGuildUser user, UserAccount account,ProfileCard card)
        {
            //ChestStyle chest = user is { PremiumSince: null }
            //?
            //    (buser.activiy_level > 10
            //    ? ChestStyle.Green
            //    : ChestStyle.Default)

            //: ChestStyle.Orange;

            Image image = Image.Load(FileManager.ToResourcePath($"Images/Chest/{"default".ToString().ToLower()}.png"));

            if (card is null || card.Chest is null)
            {
                card.Chest = new Personalchest(new List<PersonalChestItem>()) { };
            }

            var items = card.Chest.GetAsDictionary();


            for (var wIndex = 0; wIndex < wSlotSize; wIndex++)
            {
                for (var hIndex = 0; hIndex < hSlotSize; hIndex++)
                {
                    var slotIndex = hIndex * wSlotSize + wIndex;
                    if (items.ContainsKey(slotIndex))
                    {
                        var itemImgPath = FileManager.ToResourcePath($"Images/Inventory/{items[slotIndex].ItemType.ToString().ToLower()}.png");
                        if (!File.Exists(itemImgPath)) itemImgPath = FileManager.ToResourcePath($"Images/unknown.png");

                        using (Image itemImg = Image.Load(path: itemImgPath))
                        {
                            itemImg.Mutate(x => x.Resize(SlotSizeWidth - 5, SlotSizeHeight - 5, true));
                            var point = new Point(2 + (firstSlotSizeMarginWidth + (wIndex * (SlotSizeWidth + SlotSizeMarginWidth))), 2 + (firstSlotSizeMarginHeight + (hIndex * (SlotSizeHeight + SlotSizeMarginHeight))));
                            image.Mutate(x => x.DrawImage(itemImg, point, 1f));
                        }
                        var pointCount = new Point(2 + (firstSlotSizeMarginWidth + (wIndex * (SlotSizeWidth + SlotSizeMarginWidth))), 2 + (firstSlotSizeMarginHeight + (hIndex * (SlotSizeHeight + SlotSizeMarginHeight))));

                        var color = Color.White;

                        image.Mutate(x => x.DrawText(items[slotIndex].Count.ToString(), new Font(fontFamily, 24, FontStyle.Bold), color, pointCount));
                    }
                }
            }

            using (Image avatar = await _internetService.GetImageAsync(user?.GetAvatarUrl() ?? user?.GetDefaultAvatarUrl()))
            {
                avatar.Mutate(x => x.Resize(183, 183, true));
                image.Mutate(x => x.DrawImage(avatar, new Point(50, 70), 1f));
            }

            var font = new Font(fontFamily, 20, FontStyle.Regular);

            image.Mutate(x => x.DrawText(card.Profile.Level.Metric().ToString(), font, Color.White, new Point(84, 274)));

            image.Mutate(x => x.DrawText(card.Profile.Coins.Metric().ToString(), font, Color.White, new Point(175, 274)));

            image.Mutate(x => x.DrawText("Latest Activity: " + card.Profile.LastActiveAt.ToRelative() + " At " + card.Profile.LatestServer + ".", new Font(fontFamily, 16, FontStyle.Regular), Color.WhiteSmoke, new Point(40, 538)));

            var ExperienceLine = new Pen(brush: Brushes.Solid(Color.White), 9);
            image.Mutate(x => x.DrawLines(ExperienceLine, new Point(56, 316), new Point((int)(card.Profile.GetExperience(180) + 56), 316)));

            image.Mutate(x => x.DrawText("Account: " + account.Reminder.TryRemove(16), new Font(fontFamily, 14, FontStyle.Regular), Color.White, new Point(55, 371)));

            for (int wIndex = 0; wIndex < card.Profile.Badges.Count; wIndex++)
            {
                var itemImgPath = FileManager.ToResourcePath($"Images/Badges/{card.Profile.Badges[wIndex].ToString().ToLower()}.png");
                if (!File.Exists(itemImgPath)) itemImgPath = FileManager.ToResourcePath($"Images/unknown.png");

                using (Image itemImg = Image.Load(path: itemImgPath))
                {
                    itemImg.Mutate(wIndex => wIndex.Resize(23, 23));
                    var point = new Point((int)(51 + (wIndex * 27)), 332);
                    image.Mutate(x => x.DrawImage(itemImg, point, 1f));
                }
            }


            return image;

        }



    }
}
