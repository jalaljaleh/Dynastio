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
        public Image GetProfile(Profile profile)
        {
            Image image = Image.Load(FileManager.ToResourcePath("Images/Profile/default.png"));

            var pointCoinSection = new PointF() { X = (360), Y = 90 };
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

        public enum ChestStyle
        {
            Default,
            Orange
        }
        private const int wSlotSize = 6;
        private const int hSlotSize = 5;
        private const int SlotSizeWidth = 73;
        private const int SlotSizeHeight = 87;
        private const int firstSlotSizeMarginWidth = 288;
        private const int firstSlotSizeMarginHeight = 52;
        private const int SlotSizeMarginWidth = 5;
        private const int SlotSizeMarginHeight = 7;
        public async Task<Image> GetPersonalChestAsync(Discord.IGuildUser user, UserAccount account, Personalchest personalchest)
        {
            ChestStyle chest = user is { PremiumSince: null } ? ChestStyle.Default : ChestStyle.Orange;
            Image image = Image.Load(FileManager.ToResourcePath($"Images/Chest/{chest.ToString().ToLower()}.png"));

            if (personalchest == null) return image;

            var items = personalchest.GetAsDictionary();


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
                avatar.Mutate(x => x.Resize(151, 151, true));
                image.Mutate(x => x.DrawImage(avatar, new Point(68, 83), 1f));
            }
            image.Mutate(wIndex => wIndex.DrawText("User: " + user.Username, new Font(fontFamily, 16, FontStyle.Regular), Color.White, new Point(74, 250)));
            image.Mutate(wIndex => wIndex.DrawText("Account: " + account.Reminder, new Font(fontFamily, 16, FontStyle.Regular), Color.White, new Point(74, 275)));

            return image;

        }



    }
}
