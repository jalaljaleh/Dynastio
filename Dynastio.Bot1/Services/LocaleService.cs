using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Reflection;
using System.Diagnostics;

namespace Dynastio.Bot
{
    public class LocaleService
    {
        public Dictionary<string, Locale> Languages { get; set; } = new();
        public LocaleService()
        {
            Program.Log("LocaleService", "Started");

            var enFile = File.ReadAllText("master.json".ResourcesPath());
            var enJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(enFile);
            enJson = enJson.OrderBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);
            var enLocale = new Locale(enJson);
            Languages.Add("master", enLocale);
            
            foreach (var l in Directory.GetFiles("Locale".ResourcesPath()))
            {
                var langFile = File.ReadAllText(l);
                var langJson = JsonConvert.DeserializeObject<string[]>(langFile);
                string lang = Path.GetFileNameWithoutExtension(l);
                Dictionary<string, string> dictionary = new();
                for (int i = 0; i < langJson.Length; i++)
                {
                    dictionary.Add(enJson.Keys.Skip(i).First(), langJson[i]);
                }
                Languages.Add(lang, new Locale(dictionary));

            
                Program.Log("LocaleService", $"Locale {lang.PadRight(5)} Added with {enJson.Values.Count.ToString().PadRight(5)} words.");
            }
        }
        public static void StartTranslateProccess()
        {
            var enFile = File.ReadAllText("master.json".ResourcesPath());
            var enJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(enFile);
            enJson = enJson.OrderBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);
            string content = JsonConvert.SerializeObject(enJson.Select(a => a.Value).ToArray());

            WindowsClipboard.SetText(content);
            Console.WriteLine("Content copied to your Clipboard");

            do
            {
                Console.WriteLine("Write your locale code:");
                var locale_ = Console.ReadLine();

                Console.WriteLine($"Copy Translated of {locale_} to the window then close it..");

                File.WriteAllText("test.txt".ResourcesPath(), "");

                var fileToOpen = "test.txt".ResourcesPath();
                var process = new Process();
                process.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = fileToOpen,
                };
               
                process.Start();
                process.WaitForExit();

                var result = File.ReadAllText("test.txt".ResourcesPath());
              
                File.WriteAllText(@$"J:\source\jalaljaleh\Dynastio\Dynastio.Bot\Resources\Locale\{locale_}.json", result);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("File Saved in the resource.");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("\nStart new one ? [Y/N]");
            }
            while (Console.ReadLine().ToLower().Contains("y"));
        }

        public Locale this[string key]
        {
            get
            {
                if (Languages.TryGetValue(key, out Locale value))
                {
                    return value;
                };
                return Languages.Where(a => a.Key == "en").First().Value;
            }
        }

    }
    public class Locale
    {
        public readonly Dictionary<string, string> Words;
        public Locale(Dictionary<string, string> Words)
        {
            this.Words = Words;
        }
        public void WriteInConsole()
        {
            foreach (var i in Words.Keys) Console.WriteLine(i);
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
        public string this[string key, string param1]
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
                return regex.Replace(result, param1, 1);
            }
        }
        public string this[string key, int param1] { get => this[key, param1.ToString()]; }
    }
}
