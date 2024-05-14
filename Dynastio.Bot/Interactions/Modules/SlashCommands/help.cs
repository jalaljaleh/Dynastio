using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
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
    public class HelpModule : BotInteractionModuleBase
    {
        [SlashCommand("help", "help")]
        public async Task help()
        {
            var embed = new EmbedBuilder()
            {
                Title = "Help",
                Description =
                $"` /dashboard ` shows the dashboard of the bot.\n" +
                $"` /dynastio ` open dynast.io menu .\n" +
                $"` /profile ` open user profile & settings.\n" +
                $"` /setup ` open menu for server admins. \n",
                ThumbnailUrl = this.BotAvatarUrl,
            }.Build();

            await RespondAsync(userMention, embed: embed);
        }
    }
}
