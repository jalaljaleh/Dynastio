
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Global
{
    public class PathHelper
    {
        static string _currentDirectory;
        public static string CurrentDirectory
        {
            get
            {
                if (_currentDirectory == null)
                    _currentDirectory = Directory.GetCurrentDirectory();

                return _currentDirectory;
            }
        }
        public static string GetConfigFile()
        {
            return Path.Combine(CurrentDirectory, $@"assets\config.json");
        }
        public static string GetDebugFile()
        {
            return Path.Combine(CurrentDirectory, $@"W:\projects\Dynastio.Bot\debug.json");
        }
        public static string GetData()
        {
            return Path.Combine(CurrentDirectory, @"assets\data");
        }

    }
}
