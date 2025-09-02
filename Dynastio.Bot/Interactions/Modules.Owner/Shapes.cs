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
            var shapeGen = new DynastioShapeGenerator(EmoteService);

            string shape = await shapeGen.CreateRandomShapeAsync<ItemType>(width, height);


            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithTextDisplay(shape);

            await RespondAsync(components: cb.Build());
        }

    }
   

}
