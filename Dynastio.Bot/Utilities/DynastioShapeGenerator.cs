using Dynastio.Bot.Services;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class DynastioShapeGenerator
    {
        private readonly EmoteService _emoteService;
        private readonly Random _random = new();

        public DynastioShapeGenerator(EmoteService emoteService)
        {
            _emoteService = emoteService;
        }
        public string GeneratePersonalChest(PersonalChest personalChest)
        {
            // 1. Null-safe guard
            if (personalChest == null || personalChest.Items == null)
                personalChest = new PersonalChest(new List<PersonalChestItem>());

            int width = 6;
            int height = 5;
            var items = personalChest.Items.OrderBy(a=>a.Index).ToList();
            var sb = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                // Prefix each row with “# ”
                sb.Append("# ");

                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    // 2. If we have an item at this slot, render its emote; otherwise use a placeholder
                    if (index < items.Count)
                    {
                        var emote = _emoteService.GetEmote(items[index].ItemType);
                        sb.Append($"{emote} ");
                    }
                    else
                    {
                        sb.Append("▫️ ");  // Empty-slot placeholder
                    }
                }

                // New line (but not after the last row)
                if (y < height - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> CreateRandomShapeAsync<TEnum>(int width, int height)
            where TEnum : struct, Enum
        {
            // Ensure emote cache is ready
            await _emoteService.EnsureReadyAsync();

            string content = "# ";
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Pick a random enum value
                    var value = values[_random.Next(values.Length)];
                    // Get emote tag from EmoteService
                    var emoteTag = _emoteService.GetEmoteTag(value);

                    // Append emote to our row
                    content += $"|| {emoteTag} ||";
                }
                if (height - 1 > y)
                    content += "\n# ";
            }

            return content;
        }
    }
}
