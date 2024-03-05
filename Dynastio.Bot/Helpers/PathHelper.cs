using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class PathHelper
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
            return Path.Combine(CurrentDirectory, $@"config.json");
        }
        public static string GetData()
        {
            return Path.Combine(CurrentDirectory, @"data");
        }
        public static string GetEncryptedConfigFile()
        {
            return Path.Combine(CurrentDirectory, @"../../../../data");
        }
    }
}
