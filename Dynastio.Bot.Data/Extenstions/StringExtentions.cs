using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    internal static class StringExtentions
    {
        public static string TryRemove(this string value, int maxLength, bool dots = true)
        {
            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength) + (dots ? ".." : "");
            }
            return value;
        }
    }
}
