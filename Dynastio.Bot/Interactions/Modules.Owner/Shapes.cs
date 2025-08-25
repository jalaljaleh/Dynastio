using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Bot.Rank.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireTeam()]
    public class ShapeModule : MenuModulesBase
    {

        [SlashCommand("shape", "description")]
        public async Task shape(int width, int height)
        {
            var shapeGen = new ShapeGenerator(EmoteService);

            string shape = await shapeGen.CreateRandomShapeAsync<ItemType>(width, height);


            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithTextDisplay(shape);

            await RespondAsync(components: cb.Build());
        }

    }
    public class ShapeGenerator
    {
        private readonly EmoteService _emoteService;
        private readonly Random _random = new();

        public ShapeGenerator(EmoteService emoteService)
        {
            _emoteService = emoteService;
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
