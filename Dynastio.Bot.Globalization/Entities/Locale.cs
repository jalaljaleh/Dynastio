using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Bot.Globalization
{
    public class Locale
    {
        public readonly Dictionary<string, string> Words;
        public Locale(Dictionary<string, string> Words)
        {
            this.Words = Words;
        }

        public string this[string key]
        {
            get
            {
                if (!Words.TryGetValue(key, out string value))
                    return key;

                return value;
            }
        }

        public string this[string key, params object[] param]
        {
            get
            {
                if (!Words.TryGetValue(key, out string value))
                    return key;

                return string.Format(value, param);
            }
        }
    }
}
