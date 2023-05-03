using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RateLimit(60, 2, RateLimit.RateLimitType.User)]
    public class Version : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [SlashCommand("version", "dynast.io version")]
        public async Task version()
        {
            await DeferAsync();
            var version = Dynastio.Version;
            var message = await FollowupAsync(
                embed: (
                $"Dynastio Current Version Is **{version.CurrentVersion}**" +
                $"\n[Download]({version.DownloadUrl})"
                ).ToEmbed("Dynastio Version", Context.Client.CurrentUser.GetAvatarUrl()));
        }
    }
}
