using Dynastio.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Dynastio.Graphic.Extenstions;
using Dynastio.Graphic.Helpers;
using System.Net.NetworkInformation;

namespace Dynastio.Graphic
{
    public static class DynastioGraphicHelper
    {
        public static string GetUnkownPng()
        {
            return PathHelper.GetUnknown();
        }
        public static IEnumerable<string> GetEntitiesPng()
        {
            return Directory.GetFiles(PathHelper.GetEntitiesDirectory());
        }
        public static string[] GetBadgesPng()
        {
            return Directory.GetFiles(PathHelper.GetBadgesDirectory());
        }
        
        public static string[] GetItemsPng()
        {
           return Directory.GetFiles(PathHelper.GetItemsDirectory());
        }
        public static string[] GetUisPng()
        {
            return Directory.GetFiles(PathHelper.GetUiDirectory());
        }
        public static string[] GetSkinPng()
        {
            return Directory.GetFiles(PathHelper.GetSkinDirectory());
        }
    }
}
