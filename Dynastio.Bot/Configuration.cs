using Dynastio.Bot.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class Configuration
    {
        public string BotToken { get; set; }
        public string DynastioApi { get; set; }
        public string YoutubeApi { get; set; }
        public string MongodbConnection { get; set; }
        public string Prefix { get; set; }

        public ulong DebugServerId { get; set; }




        public static Configuration LoadConfiguration()
        {
            if (Global.Main.IsDebug())
            {
                var file = File.ReadAllText(FileManager.ToResourcePath(@"debug-config.json"));
                return JsonConvert.DeserializeObject<Configuration>(file);
            }

            if (File.Exists(FileManager.ToResourcePath(@"config-key.txt")))
            {
                var _configKey = File.ReadAllText(FileManager.ToResourcePath(@"config-key.txt"));
                Environment.SetEnvironmentVariable("config-key", _configKey);

            }
            if (File.Exists(FileManager.ToResourcePath(@"release-config.json")))
            {
                var txtConfig = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(FileManager.ToResourcePath(@"release-config.json")));

                var configKey = File.ReadAllText(FileManager.ToResourcePath(@"config-key.txt"));
                var encryptedConfig = Encryption.Encrypt(JsonConvert.SerializeObject(txtConfig), configKey);

                File.WriteAllText(FileManager.ToResourcePath(@"../../../../Resources/configuration"), encryptedConfig);

                Environment.SetEnvironmentVariable("config-key", configKey);
            }
          

            string? key = Environment.GetEnvironmentVariable("config-key");
            if (key is null)
            {
                throw new Exception(" Config-Key not found from environment variables.");
            }

            string encryptedvalue = File.ReadAllText(FileManager.ToResourcePath(@"configuration"));
            string clearValue = Encryption.Decrypt(encryptedvalue, key);

            var value = JsonConvert.DeserializeObject<Configuration>(clearValue);
            if (value is null)
            {
                throw new Exception($" Configuration value is null.");
            }

            return value;
        }
    }
}
