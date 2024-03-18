using System.Text.RegularExpressions;

namespace Dynastio.Extenstions
{
    public static class StringExtensions
    {

        public static string Join(this IEnumerable<string> value, string spearator)
        {
            return string.Join(spearator, value);
        }

        public static string Remove(this string value, params string[] txt)
        {
            foreach (var x in txt)
                value = value.Replace(x, "");
            return value;
        }
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


        public static string ToSafeChars(this string value)
        {
            string inputString = value;
            foreach (var entry in replacements)
            {
                string pattern = entry.Key;
                string replacement = entry.Value;

                inputString = Regex.Replace(inputString, pattern, replacement, RegexOptions.IgnoreCase);
            }
            return inputString;
        }

        private static readonly Dictionary<string, string> replacements = new()
    {
        {"[áàäâãåāăąǎǟ]", "a"},
        {"[ÁÀÄÂÃÅĀĂĄǍǞ]", "A"},
        {"(æ|ǽ)", "ae"},
        {"(Æ|Ǽ)", "AE"},
        {"[çćĉċč]", "c"},
        {"[ÇĆĈĊČ]", "C"},
        {"[ðďđ]", "d"},
        {"[ÐĎĐ]", "D"},
        {"[éèëêēĕėęě]", "e"},
        {"[ÉÈËÊĒĔĖĘĚ]", "E"},
        {"[ƒ]", "f"},
        {"[Ƒ]", "F"},
        {"[ĝğġģ]", "g"},
        {"[ĜĞĠĢ]", "G"},
        {"[ĥħ]", "h"},
        {"[ĤĦ]", "H"},
        {"[íìïîīĭįıǐ]", "i"},
        {"[ÍÌÏÎĪĬĮİǏ]", "I"},
        {"[ĵ]", "j"},
        {"[Ĵ]", "J"},
        {"[ķ]", "k"},
        {"[Ķ]", "K"},
        {"[łĺļľŀ]", "l"},
        {"[ŁĹĻĽĿ]", "L"},
        {"[ñńņňŉŋ]", "n"},
        {"[ÑŃŅŇŊ]", "N"},
        {"[óòöôõøōŏőǒǫǿ]", "o"},
        {"[ÓÒÖÔÕØŌŎŐǑǪǾ]", "O"},
        {"(œ)", "oe"},
        {"(Œ)", "OE"},
        {"[ŕŗř]", "r"},
        {"[ŔŖŘ]", "R"},
        {"[śšşŝș]", "s"},
        {"[ŚŠŞŜȘ]", "S"},
        {"(ß)", "ss"},
        {"[ťţŧț]", "t"},
        {"[ŤŢŦȚ]", "T"},
        {"[úùüûūŭůűųǔǖǘǚǜ]", "u"},
        {"[ÚÙÜÛŪŬŮŰŲǓǕǗǙǛ]", "U"},
        {"[ṽ]", "v"},
        {"[Ṽ]", "V"},
        {"[ŵ]", "w"},
        {"[Ŵ]", "W"},
        {"[ýÿŷ]", "y"},
        {"[ÝŸŶ]", "Y"},
        {"[źżž]", "z"},
        {"[ŹŻŽ]", "Z"}};

    }
}
