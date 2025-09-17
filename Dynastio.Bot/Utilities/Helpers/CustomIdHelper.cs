using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class CustomIdHelper
    {
        public static string Generate()
        {
            return Common.Random.Next(-999999999, 999999999).ToString();
        }
    }
}
