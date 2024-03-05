using Dynastio.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Dynastio.Graphic.Extenstions;
using Dynastio.Graphic.Helpers;


namespace Dynastio.Graphic
{
    public class DynastioGraphic
    {
        private Font _font;
        private FontFamily _fontFamily;
        private readonly InternetService _internetService;
        public DynastioGraphic()
        {
            _internetService = new InternetService();
        }
        public DynastioGraphic Initialize()
        {
            //Main.Log("Graphic Service", "Initializing ..");

            FontCollection collection = new FontCollection();
            _fontFamily = collection.Add(PathHelper.GetFont("FiraSans-Bold"));
            _font = _fontFamily.CreateFont(12, FontStyle.Bold);

            //Main.Log("Graphic Service", "Initializied");

            return this;
        }
        public async Task<Image> GetWelcomeImageAsync(string userAvatar, string username, int memberth)
        {
            Image image = Image.Load(PathHelper.GetWelcome());

            using (Image avatar = await _internetService.GetImageAsync(userAvatar))
            {
                avatar.Mutate(wIndex => wIndex.Resize(238, 238));
                image.Mutate(wIndex => wIndex.DrawImage(avatar, new Point(30, 30), 1f));

                image.Mutate(wIndex => wIndex.DrawText("Username: " + username, new Font(_fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 300)));
                image.Mutate(wIndex => wIndex.DrawText("Member: " + memberth + "th", new Font(_fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 370)));
                image.Mutate(wIndex => wIndex.DrawText("Welcome to Dynast.io", new Font(_fontFamily, 35, FontStyle.Regular), Color.White, new Point(40, 430)));
            }

            return image;
        }

        private const int wSlotSize = 6;
        private const int hSlotSize = 5;
        private const int SlotSizeWidth = 73;
        private const int SlotSizeHeight = 87;
        private const int firstSlotSizeMarginWidth = 288;
        private const int firstSlotSizeMarginHeight = 52;
        private const int SlotSizeMarginWidth = 5;
        private const int SlotSizeMarginHeight = 7;
        public async Task<Image> GetProfileImageAsync(string userAvatar, string account, ProfileCard card)
        {
            Image image = Image.Load(PathHelper.GetThemes("default"));

            if (card is null || card.Chest is null)
                card.Chest = new Personalchest(new List<PersonalChestItem>()) { };
            

            // Items part
            var items = card.Chest.GetAsDictionary();
            for (var wIndex = 0; wIndex < wSlotSize; wIndex++)
            {
                for (var hIndex = 0; hIndex < hSlotSize; hIndex++)
                {
                    var slotIndex = hIndex * wSlotSize + wIndex;
                    if (items.ContainsKey(slotIndex))
                    {
                        var itemImgPath = PathHelper.GetItem(items[slotIndex].ItemType);
                        if (!File.Exists(itemImgPath)) itemImgPath = PathHelper.GetUnknown();

                        using (Image itemImg = Image.Load(path: itemImgPath))
                        {
                            itemImg.Mutate(x => x.Resize(SlotSizeWidth - 5, SlotSizeHeight - 5, true));
                            var point = new Point(2 + (firstSlotSizeMarginWidth + (wIndex * (SlotSizeWidth + SlotSizeMarginWidth))), 2 + (firstSlotSizeMarginHeight + (hIndex * (SlotSizeHeight + SlotSizeMarginHeight))));
                            image.Mutate(x => x.DrawImage(itemImg, point, 1f));
                        }
                        var pointCount = new Point(2 + (firstSlotSizeMarginWidth + (wIndex * (SlotSizeWidth + SlotSizeMarginWidth))), 2 + (firstSlotSizeMarginHeight + (hIndex * (SlotSizeHeight + SlotSizeMarginHeight))));

                        var color = Color.White;

                        image.Mutate(x => x.DrawText(items[slotIndex].Count.ToString(), new Font(_fontFamily, 24, FontStyle.Bold), color, pointCount));
                    }
                }
            }


            // Avatar part
            using (Image avatar = await _internetService.GetImageAsync(userAvatar))
            {
                avatar.Mutate(x => x.Resize(183, 183, true));
                image.Mutate(x => x.DrawImage(avatar, new Point(50, 70), 1f));
            }


            // Details part
            var font = new Font(_fontFamily, 20, FontStyle.Regular);
            image.Mutate(x => x.DrawText(card.Profile.Level.Metric().ToString(), font, Color.White, new Point(84, 274)));
            image.Mutate(x => x.DrawText(card.Profile.Coins.Metric().ToString(), font, Color.White, new Point(175, 274)));
            image.Mutate(x => x.DrawText("Latest Activity: " + card.Profile.LastActiveAt.ToRelative() + " At " + card.Profile.LatestServer + ".", new Font(_fontFamily, 16, FontStyle.Regular), Color.WhiteSmoke, new Point(40, 538)));

            var ExperienceLine = new Pen(brush: Brushes.Solid(Color.White), 9);
            image.Mutate(x => x.DrawLines(ExperienceLine, new Point(56, 316), new Point((int)(card.Profile.GetExperience(180) + 56), 316)));
            image.Mutate(x => x.DrawText("Account: " + account.TryRemove(16), new Font(_fontFamily, 14, FontStyle.Regular), Color.White, new Point(55, 371)));

            // Badges part
            for (int wIndex = 0; wIndex < card.Profile.Badges.Count; wIndex++)
            {
                var itemImgPath = PathHelper.GetBadge(card.Profile.Badges[wIndex]);
                if (!File.Exists(itemImgPath)) itemImgPath = PathHelper.GetUnknown();

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
