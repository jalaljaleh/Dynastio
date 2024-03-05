using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Dynastio.Bot.Globalization
{
    public class DynastioBotGlobalization
    {
        internal Dictionary<string, Locale> Languages { get; set; } = new();
        public void Initialize()
        {
            foreach (var file in Directory.GetFiles(PathHelper.GetDiretory()))
            {
                var langFile = File.ReadAllText(file);
                var locale = JsonConvert.DeserializeObject<Dictionary<string, string>>(langFile);

                string lang = Path.GetFileNameWithoutExtension(file);

                Languages.Add(lang, new Locale(locale));

                //Global.Main.Log("Globalization", $"{lang.PadRight(5)} loaded.");
            }
        }
        public Locale GetOrDefault(string key)
        {
            if (Languages.TryGetValue(key, out Locale value))
                return value;

            return Languages.Where(a => a.Key == "en").First().Value;
        }
        public Locale this[string key]
        {
            get => GetOrDefault(key);
        }
    }

}