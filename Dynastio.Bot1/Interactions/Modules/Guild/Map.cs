using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Shard;
using Dynastio.Data;
using Dynastio.Net;
namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles | ChannelPermission.SendMessages)]
    public class MapModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }

        [RateLimit(8, 2, RateLimit.RateLimitType.User)]
        [SlashCommand("map", "the mini map")]
        public async Task Map(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
            DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();

            var selectedServer = Dynastio[provider].OnlineServers.FirstOrDefault(a => a.Label == server);
            if (selectedServer is null)
            {
                await FollowupAsync(embed: "No any online server found.".ToWarnEmbed("Not Found !"));
                return;
            }

            var map = GraphicService.GetMap(selectedServer.Players);

            var embed = $"{selectedServer.Players.Count} players.".ToWarnEmbed($"{server} Map", imageUrl: "attachment://map.jpeg");

            await FollowupWithFileAsync(map, "map.jpeg", Context.User.Id.ToUserMention(), embed: embed);
        }

    }
}
