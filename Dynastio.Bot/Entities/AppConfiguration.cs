using Dynastio.Bot.Global;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Entities
{
    public class AppConfiguration 
    {
        public Dictionary<string, string> Tokens = new Dictionary<string, string>();
        public ulong DebugServerId { get; set; }

        const string _configPath = "config.json";
        public static AppConfiguration LoadConfiguration(/*bool addLocalEnv*/)
        {

            ///// Encrypted Config
            //// add encrypt-key to environment
            //if (addLocalEnv && File.Exists(keypath))
            //{
            //    var _configKey = File.ReadAllText(keypath);
            //    Environment.SetEnvironmentVariable("config-key", _configKey);
            //}

            //if (string.IsNullOrEmpty(key))
            //    throw new Exception(" Config-Key not found from environment variables.");


            var value = File.ReadAllText(PathHelper.GetConfigFile());
            var clearValue = Encryption.Decrypt(value, Environment.GetEnvironmentVariable("config-key"));
            var config = JsonConvert.DeserializeObject<AppConfiguration>(value);
           
            //WriteEncryptedConfiguration(config);

            return config ?? throw new Exception("null config");
        }

        public static void WriteEncryptedConfiguration(AppConfiguration config)
        {
            try
            {
                Console.WriteLine("\n\n Write Your Encryption Key >:");
                var _configKey = Console.ReadLine();

                var serializeConfig = JsonConvert.SerializeObject(config);
                var encryptedConfig = Encryption.Encrypt(serializeConfig, _configKey);

                File.WriteAllText(PathHelper.GetEncryptedConfigFile(), encryptedConfig);

                Global.Main.Log("Config Service", "Configuration file updated succesfully.");
            }
            catch
            {
                Global.Main.Log("Config Service", "Can't update the configuration.");
            }
        }
    }
}
