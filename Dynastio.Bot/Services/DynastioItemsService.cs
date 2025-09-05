using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Dynastio.Bot
{
    public sealed class DynastioItemsService
    {
        // In‐memory store of loaded items
        private Dictionary<ItemType, DynastioItem> _items = new(0);

        // Cached name→ItemType map (case-insensitive)
        private Dictionary<string, ItemType> _nameToType = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Call this at startup (or on demand) to load/refresh items.json into memory.
        /// </summary>
        public void Initialize()
        {
            // Compose path: <CurrentDirectory>/assets/game/items.json
            var filePath = Path.Combine(Environment.CurrentDirectory, "assets", "game", "items.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find items.json", filePath);

            // Read entire JSON payload
            var json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            var root = JsonSerializer.Deserialize<DynastioItemsRoot>(json, options)
                       ?? throw new InvalidDataException("Invalid items.json format");

            // Build name→ItemType map once
            var nameToType = new Dictionary<string, ItemType>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemType t in Enum.GetValues<ItemType>())
                nameToType[t.ToString()] = t;

            // Pre-size the dictionary
            var dict = new Dictionary<ItemType, DynastioItem>(root.Items.Count);

            foreach (var kv in root.Items)
            {
                if (nameToType.TryGetValue(kv.Key, out var type))
                    dict[type] = kv.Value;
                // unknown names are skipped
            }

            // Swap dictionaries atomically
            _items = dict;
            _nameToType = nameToType;
        }

        /// <summary>
        /// Get the item for this type, or throw if missing.
        /// </summary>
        public DynastioItem GetItem(ItemType type)
        {
            if (_items.TryGetValue(type, out var item))
                return item;

            throw new KeyNotFoundException($"No item defined for type {type}");
        }

        /// <summary>
        /// Try-get pattern: returns false if the item isn't loaded.
        /// </summary>
        public bool TryGetItem(ItemType type, out DynastioItem? item)
            => _items.TryGetValue(type, out item!);

        /// <summary>
        /// Get the item by its string name (case-insensitive), or throw if missing.
        /// </summary>
        public DynastioItem GetItem(string name)
        {
            if (_nameToType.TryGetValue(name, out var type))
                return GetItem(type);

            throw new KeyNotFoundException($"No item defined with name '{name}'");
        }

        /// <summary>
        /// Try-get pattern by string name (case-insensitive).
        /// </summary>
        public bool TryGetItem(string name, out DynastioItem? item)
        {
            if (_nameToType.TryGetValue(name, out var type))
                return _items.TryGetValue(type, out item!);

            item = null;
            return false;
        }

        /// <summary>
        /// Returns a random item from the loaded collection.
        /// </summary>
        public DynastioItem GetRandomItem()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("No items are loaded. Call Initialize() first.");

            int index = Common.Random.Next(_items.Count);

            // Enumerate to the chosen index
            int i = 0;
            foreach (var kvp in _items)
            {
                if (i == index)
                    return kvp.Value;
                i++;
            }

            // Should never get here
            throw new InvalidOperationException("Failed to select a random item.");
        }
    }
}
