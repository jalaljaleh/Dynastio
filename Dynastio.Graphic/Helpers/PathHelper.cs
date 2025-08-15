using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Graphic.Helpers
{
    internal class PathHelper
    {
        static string _currentDirectory;
        static string CurrentDirectory
        {
            get
            {
                if (_currentDirectory == null)
                    _currentDirectory = Directory.GetCurrentDirectory();

                return _currentDirectory;
            }
        }

        public static string GetFile(string path)
        {
            return Path.Combine(CurrentDirectory, $@"assets/{path}");
        }
        public static string GetFont(string name)
        {
            return Path.Combine(CurrentDirectory, $@"assets/fonts/{name}.ttf");
        }
        public static string GetBadge(BadgeType badge)
        {
            return Path.Combine(CurrentDirectory, $@"assets/game/badges/{badge.ToString().ToLower()}.png");
        }
        public static string GetItem(ItemType item)
        {
            return Path.Combine(CurrentDirectory, $@"{GetItemsDirectory()}{item.ToString().ToLower()}.png");
        }
        public static string GetEntitiesDirectory()
        {
            return Path.Combine(CurrentDirectory, $@"{GetGameDirectory()}/entities/");
        }
        public static string GetItemsDirectory()
        {
            return Path.Combine(CurrentDirectory, $@"{GetGameDirectory()}/items/");
        }
        public static string GetBadgesDirectory()
        {
            return Path.Combine(CurrentDirectory, $@"{GetGameDirectory()}/badges/");
        }
        public static string GetGameDirectory()
        {
            return Path.Combine(CurrentDirectory, @"assets/game/");
        }
        public static string GetEntity(EntityType entity)
        {
            return Path.Combine(CurrentDirectory, $@"assets/game/entities/{entity.ToString().ToLower()}.png");
        }
        public static string GetThemes(string name)
        {
            return Path.Combine(CurrentDirectory, $@"assets/themes/profile/{name}.png");
        }
        public static string GetUnknown()
        {
            return Path.Combine(CurrentDirectory, $@"assets/game/unknown.png");
        }
        public static string GetWelcome()
        {
            return Path.Combine(CurrentDirectory, $@"assets/themes/welcome.jpg");
        }
    }
}
