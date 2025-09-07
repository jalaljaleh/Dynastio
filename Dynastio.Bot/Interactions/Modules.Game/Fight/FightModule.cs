using Discord.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Game.Fight
{
    public class FightModule : MenuModulesBase
    {
        [RequireLinkedAccountAttribute]
        [SlashCommand("pvp", "description")]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var fighter = Context.BotUser.GetDefaultAccount();
            
        }
    }
}
