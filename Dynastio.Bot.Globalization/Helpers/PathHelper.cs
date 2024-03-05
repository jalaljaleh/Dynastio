using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Globalization
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
        public static string GetDiretory()
        {
            return Path.Combine(CurrentDirectory, $@"assets/globalization");
        }
        
    }
}