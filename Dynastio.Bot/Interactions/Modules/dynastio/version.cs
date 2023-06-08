using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.modules.dynastio
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RateLimit(4)]
    public class VersionModule : CustomInteractionModuleBase
    {
        public DynastioClient _dynastio { get; set; }

        [SlashCommand("version", "dynast.io version")]
        public async Task version()
        {
            await DeferAsync();
            var version = _dynastio.Version;
            var message = await FollowupAsync(
                embed:
                $"Current version is Is **{version.CurrentVersion}** [Download]({version.DownloadUrl})"
                .ToEmbed("Version", Context.Client.CurrentUser.GetAvatarUrl()));
        }
    }
}
