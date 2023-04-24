using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules
{

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class Ping : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [RateLimit(5, 1, RateLimit.RateLimitType.User)]
        [SlashCommand("ping", "ping the bot")]
        public async Task ping()
        {
            await DeferAsync();
            await FollowupAsync(
                embed:
                $"Roundtrip latency: ` {Context.Client.Latency} `, Startup: {Program.StartUp.ToDiscordUnixTimestampFormat()}"
                .ToSuccessfulEmbed("Pong !", Context.Client.CurrentUser.GetAvatarUrl()));
        }
    }

}
