using Dynastio.Bot.Global;
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
        public string DatabaseConnectionString { get; set; }
        public string Prefix { get; set; }

        public ulong DebugServerId { get; set; }

        static string releasefilepath { get => FileManager.ToResourcePath(@"release-config.json"); }
        static string debugfilepath { get => FileManager.ToResourcePath(@"debug-config.json"); }
        static string key { get => Environment.GetEnvironmentVariable("config-key"); }
        static string configfilepath { get => FileManager.ToResourcePath(@"configuration"); }
        static string keypath { get => FileManager.ToResourcePath(@"config-key.txt"); }
        static string config { get => File.ReadAllText(configfilepath); }

        public static void UpdateConfiguration(Configuration config)
        {
            try
            {
                var _configKey = File.ReadAllText(keypath);
                var serializeConfig = JsonConvert.SerializeObject(config);
                var encryptedConfig = Encryption.Encrypt(serializeConfig, _configKey);
                File.WriteAllText(FileManager.ToResourcePath(@"../../../../Resources/configuration"), encryptedConfig);
                
                Global.Main.Log("Config Service", "Configuration file updated succesfully.");
            }
            catch
            {
                Global.Main.Log("Config Service", "Can't update the configuration.");
            }
        }
        public static Configuration LoadReleaseConfiguration()
        {
            var file = File.ReadAllText(releasefilepath);
            return JsonConvert.DeserializeObject<Configuration>(file);
        }
        public static Configuration LoadDebugConfig()
        {
            var file = File.ReadAllText(debugfilepath);
            return JsonConvert.DeserializeObject<Configuration>(file);
        }
        public static Configuration LoadConfiguration(bool setEnvironment = false)
        {
            // set config-key value to environment
            if (setEnvironment && File.Exists(keypath))
            {
                var _configKey = File.ReadAllText(keypath);
                Environment.SetEnvironmentVariable("config-key", _configKey);
            }

            if (string.IsNullOrEmpty(key))
                throw new Exception(" Config-Key not found from environment variables.");

            var clearValue = Encryption.Decrypt(config, key);
            var value = JsonConvert.DeserializeObject<Configuration>(clearValue);
            if (value is null)
                throw new Exception($" Configuration value is null.");

            return value;
        }
    }
}
