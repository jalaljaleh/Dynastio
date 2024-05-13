using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.Buttons.bot;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{
    [RequireContext(ContextType.Guild)]
    public class MeModule : BotInteractionModuleBase
    {
        public InteractionService InteractionService { get; set; }

        [SlashCommand("me", "me")]
        [RateLimit(6, 1)]
        public async Task me()
        {
            
            await InteractionService.SlashCommands
              .FirstOrDefault(a => a.Name == "me")
              .ExecuteAsync(Context, services);
           
        }

    
    }
}
