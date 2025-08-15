using Dynastio.Bot.Global;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    /// <summary>
    /// Handles loading and encrypting configuration data for the bot.
    /// </summary>
    public class ConfigurationService
    {
        public Dictionary<string, string> Tokens { get; set; } = new();
        public string DebugDiscordBotToken{ get; set; }
        public ulong DebugServerId { get; set; }
        public int GuildLimit { get; set; } = 97;

        private const string ConfigKeyEnv = "config-key";

        /// <summary>
        /// Loads the configuration based on the current environment (debug or production).
        /// </summary>
        public static ConfigurationService Load()
        {
            return Common.IsDebug()
                ? LoadFromFile(PathHelper.GetDebugFile())
                : LoadEncrypted(PathHelper.GetData(), GetEncryptionKey());
        }

        /// <summary>
        /// Loads configuration from a plain JSON file.
        /// </summary>
        public static ConfigurationService LoadFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configuration file not found: {path}");

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<ConfigurationService>(json);

            return config ?? throw new InvalidOperationException("Configuration deserialization failed.");
        }

        /// <summary>
        /// Loads configuration from an encrypted file.
        /// </summary>
        public static ConfigurationService LoadEncrypted(string path, string encryptionKey)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Encrypted configuration file not found: {path}");

            var encrypted = File.ReadAllText(path);
            var decrypted = Encryption.Decrypt(encrypted, encryptionKey);
            var config = JsonConvert.DeserializeObject<ConfigurationService>(decrypted);

            return config ?? throw new InvalidOperationException("Decrypted configuration is invalid.");
        }

        /// <summary>
        /// Writes the encrypted configuration to the console using a user-provided key.
        /// </summary>
        public static void WriteEncrypted(ConfigurationService config)
        {
            Console.WriteLine("\n\nEnter Encryption Key:");
            var key = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(key))
            {
                Common.Log("Config Service", "Encryption key is empty.");
                return;
            }

            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                var encrypted = Encryption.Encrypt(json, key);

                Console.WriteLine("\nEncrypted Configuration:\n");
                Console.WriteLine(encrypted);

                Common.Log("Config Service", "Configuration encrypted successfully.");
            }
            catch (Exception ex)
            {
                Common.Log("Config Service", $"Encryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the encryption key from environment variables.
        /// </summary>
        private static string GetEncryptionKey()
        {
            var key = Environment.GetEnvironmentVariable(ConfigKeyEnv);
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Encryption key not found in environment variables.");

            return key;
        }
    }
}