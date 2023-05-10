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
                string result;
                if (Words.TryGetValue(key, out string value))
                {
                    result = value;
                }
                else
                {
                    result = key;
                };
                return result;
            }
        }
        public string this[string key, params object[] param]
        {
            get
            {
                string result;
                if (Words.TryGetValue(key, out string value))
                {
                    result = value;
                }
                else
                {
                    result = key;
                };
                var regex = new Regex(Regex.Escape("[x]"));

                for (int i = 0; i <= param.Length; i++)
                    result = regex.Replace(result, (string)param[i], 1);

                return result;
            }
        }
    }
}
