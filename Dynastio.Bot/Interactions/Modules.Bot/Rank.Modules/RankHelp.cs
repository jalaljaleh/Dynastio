using Discord;
using Discord.Interactions;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Bot.Ranking.Modules
{
    //[RequireContext(ContextType.Guild)]
    public class RankHelpModule : BotInteractionModuleBase
    {
        [SlashCommand("rank-help", "description")]
        public async Task rankHelp()
        {

            var embed = new EmbedBuilder()
                .WithTitle(this["rank-help.embed.title"])
                .WithDescription(this["rank-help.embed.description"])
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithColor(Color.DarkBlue)

                .WithFooter(footer => footer.Text = "Dynast.io Bot • XP System")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build();

            await RespondAsync(this.Context.User.Mention, embed: embed);
        }
    }


}
