using Dynastio.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Dynastio.Bot.Managers;
using Dynastio.Bot.Global;

namespace Dynastio.Bot
{
    public class GraphicService
    {
        public GraphicService()
        {

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

        public Image GetProfile(Profile profile)
        {
            Image image = Image.Load(FileManager.ToResourcePath($@"Images/Profile/default.png"));

            var pointCoinSection = new PointF() { X = (360 ), Y = 90 };
            image.Mutate(wIndex => wIndex.DrawText(profile.Coins.ToString(), new Font(fontFamily, 40, FontStyle.Regular), Color.White, pointCoinSection));

            var TextLevelPoint = new PointF() { X = (140 - (4 * profile.Details.Level.ToString().Length)), Y = 115 };
            image.Mutate(wIndex => wIndex.DrawText(profile.Details.Level.ToString(), new Font(fontFamily, 40, FontStyle.Bold), Color.White, TextLevelPoint));

            var pointDetailsSection = new PointF() { X = 80, Y = 260 };
            string TextDetails = string.Format("{0} was playing in {1}", profile.LastActiveAt.ToRelative(), StringExtensions.TrySubstring(profile.LatestServer, 16));
            image.Mutate(wIndex => wIndex.DrawText(TextDetails, new Font(fontFamily, 16, FontStyle.Bold), Color.WhiteSmoke, pointDetailsSection));

            var ExperienceLine = new Pen(Color.WhiteSmoke, 11);
            image.Mutate(wIndex => wIndex.DrawLines(ExperienceLine, new Point(250, 228), new Point((int)(profile.GetExperience(490) + 246), 228)));

            for (int wIndex = 0; wIndex < profile.Badges.Count; wIndex++)
            {
                var itemImgPath = FileManager.ToResourcePath($"Images/Badges/{profile.Badges[wIndex].ToString().ToLower()}.png");
                if (!File.Exists(itemImgPath)) itemImgPath = FileManager.ToResourcePath($"Images/unknown.png");

                using (Image itemImg = Image.Load(path: itemImgPath))
                {
                    itemImg.Mutate(wIndex => wIndex.Resize(32, 32));
                    var point = new Point((int)(260 + (wIndex * 44)), 177);
                    image.Mutate(wIndex => wIndex.DrawImage(itemImg, point, 1f));
                }
            }
            return image;
        }


        public Image GetPersonalChest(Personalchest personalchest)
        {

            Image image = Image.Load(FileManager.ToResourcePath($@"Images/personalchest/default.png"));

            if (personalchest == null) return image;

            var items = personalchest.GetAsDictionary();

            const int wSlotSize = 6;
            const int hSlotSize = 5;
            const int SlotSizeWidth = 73;
            const int SlotSizeHeight = 87;
            const int firstSlotSizeMarginWidth = 170;
            const int firstSlotSizeMarginHeight = 52;
            const int SlotSizeMarginWidth = 5;
            const int SlotSizeMarginHeight = 7;

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
            return image;

        }



    }
}
