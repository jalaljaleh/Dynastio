using Discord.Interactions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Modules
{
    internal class ModuleService
    {
        public async Task AddInteractionsModules(IServiceProvider services, InteractionService interactionService)
        {
            await interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
        }
    }
}
