using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.dynastio.Commands
{
    public partial class DynastioModule
    {
        [SlashCommand("version", "dynast.io version")]
        public async Task version()
        {
            await DeferAsync();
            var version = _dynastio.Version;
            var message = await FollowupAsync(
                embed: (
                $"_dynastio Current deleteMessage Is **{version.CurrentVersion}**" +
                $"\n[Download]({version.DownloadUrl})"
                ).ToEmbed("_dynastio deleteMessage", Context.Client.CurrentUser.GetAvatarUrl()));
        }
    }
}
