using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
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
                Description = "test",
                ThumbnailUrl = this.BotAvatarUrl,
            }.Build();

            await RespondAsync(this.Context.User.Mention, embed: embed);
        }
    }
}
