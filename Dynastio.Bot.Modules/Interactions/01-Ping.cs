using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Interactions;

namespace Discord.Bot.Modules.Interactions
{
    public class PingModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [SlashCommand("test", "ping")]
        public async Task ping()
        {
            await RespondAsync($"Pong !, Current Bot Latency: {Context.Client.Latency}");
        }

      
    }

}
