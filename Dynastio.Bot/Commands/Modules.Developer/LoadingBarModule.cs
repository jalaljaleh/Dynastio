using Discord.Commands;
using Dynastio.Bot.Services;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands.Modules.Developer
{
    [RequireContext(ContextType.Guild)]
    [RequireApplicationTeamAttribute()]
    public class LoadingBarModule : ModuleBase<BotSocketCommandContext>
    {
        public EmoteService EmoteService { get; set; }

        [Command("loadingbar")]
        public async Task loadingBarAsync(int barLength, int delaySeconds=2000)
        {
            const int maxUnits = 100;
            var ms = await ReplyAsync("working ..");
            for (int current = 0; current <= maxUnits; current += 10)
            {
                await Task.Delay(delaySeconds);
                string bar = EmoteService.BuildProgressBar(barLength, current, maxUnits);
                await ms.ModifyAsync(x => x.Content = bar);
            }
        }


    }
}
