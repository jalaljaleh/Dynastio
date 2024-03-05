using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Graphic.Extenstions
{
    internal static class StringExtensions
    {
        public static string RemoveLines(this string Value)
        {
            return Regex.Replace(Value, "\\t|\\n|\\r|\\r\\n|\\n\\r|", "");
        }

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
