using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Shard
{
    public class SharedModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {

        [ComponentInteraction("shared.close")]
        public async Task close()
        {
            await DeferAsync();
            await ModifyToClosed();
        }
        [ComponentInteraction("shared.remove:*")]
        public async Task remove(string value)
        {
            await DeferAsync();
            switch (value)
            {
                case "component":
                    await ToNoComponent();
                    break;
            }
        }
    }
}
