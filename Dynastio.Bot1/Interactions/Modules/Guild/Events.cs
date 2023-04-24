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
    [RateLimit(60, 1, RateLimit.RateLimitType.User)]
    public class Events : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [SlashCommand("events", "dynast.io events")]
        public async Task events(string server = "", DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            server = server.ToLower();
            var onlineServers = Dynastio[provider].OnlineServers.Where(a => !a.IsPrivate && a.Label.ToLower().Contains(server)).ToList();
            var events = onlineServers.SelectMany(a => a.Events).GroupBy(a => a.id).ToList();
            string content = events.ToStringTable(new string[] { "Event", "Coef", "Started At", "End At", "Servers" },
                a => a.First().kind.type,
                a => a.First().kind.coef,
                a => a.First().start_time.ToRelative(),
                a => a.First().finish_time.ToRelative(),
                a => a.Count() + " Servers").ToMarkdown();
            var message = await FollowupAsync(embed: content.ToEmbed("Current Events"));
        }
    }
}
