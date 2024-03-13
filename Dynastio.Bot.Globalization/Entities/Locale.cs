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
        private readonly string locale;
        private readonly Locale _default;
        public readonly Dictionary<string, string> Words;
        public Locale(Dictionary<string, string> Words, Locale @default, string locale)
        {
            this.Words = Words;
            this._default = @default;
            this.locale = locale;
        }
        public string GetTranslation(string key, params object[] param)
        {
            if (!Words.TryGetValue(key, out string value))
            {
                if (locale != "en")
                    return _default[key, param];
                else
                    return key;
            }

            if (param is null || param.Length == 0)
                return value;

            try
            {
                return string.Format(value, param);
            }
            catch
            {
                return key;
            }
        }
        public string this[string key]
        {
            get
            {
                return GetTranslation(key);
            }
        }

        public string this[string key, params object[] param]
        {
            get
            {
                return GetTranslation(key, param);
            }
        }
    }
}
