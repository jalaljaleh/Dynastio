using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;

namespace Dynastio.Bot.Interactions.Modules
{
    public class PingModule : CustomInteractionModuleBase
    {

        [SlashCommand("ping", "ping")]
        public async Task ping()
        {
            await RespondAsync(
                text: userMention,
                embed: new EmbedBuilder()
                {
                    Title = "Pong !",
                    Description =
                    $"Version: ` {Main.version} `\n" +
                    $"StartUp: {Main.StartUp.ToDiscordUnixTimestampFormat()}\n" +
                    $"",
                    ThumbnailUrl =
                    Context.Client.CurrentUser.GetAvatarUrl() ??
                    Context.Client.CurrentUser.GetDefaultAvatarUrl() ??
                    Context.Guild.IconUrl,
                }.Build());
        }

    }

}
