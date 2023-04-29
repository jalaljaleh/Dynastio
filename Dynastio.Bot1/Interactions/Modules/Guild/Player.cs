using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Shard;

namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles | ChannelPermission.SendMessages)]
    [Group("player", "online players")]
    public class PlayerModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GraphicService GraphicService { get; set; }
        public DynastioClient Dynastio { get; set; }
        public UserService UserService { get; set; }

        [RateLimit(100, 6)]
        [SlashCommand("profile", "online player profile")]
        public async Task profile(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlinePlayersAutocompleteHandler))] string player = "",
             DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var player_ = Dynastio[provider].OnlinePlayers.FirstOrDefault(a => a.UniqeId == player);
            if (player_ is null)
            {
                await FollowupAsync(embed: "player not found".ToWarnEmbed("Not Found !", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            if (!player_.IsAuth)
            {
                await FollowupAsync(embed: "the player is a guest".ToWarnEmbed("guest !", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            var profile = await Dynastio[provider].GetUserProfileAsync(player_.Id).TryGet();
            if (profile == null)
            {
                await FollowupAsync(embed: "profile not found, join the game and play a while.".ToWarnEmbed("not found", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            var image = GraphicService.GetProfile(profile);
            await FollowupWithFileAsync(image, "profile.jpeg", $"Online player profile ({player_.Nickname})");
        }

        [RateLimit(100, 6)]
        [SlashCommand("chest", "online player chest")]
        public async Task chest(
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlineServersAutocompleteHandler))] string server = "",
            [Autocomplete(typeof(SharedAutocompleteHandler.OnlinePlayersAutocompleteHandler))] string player = "",
             DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var player_ = Dynastio[provider].OnlinePlayers.FirstOrDefault(a => a.UniqeId == player);
            if (player_ is null)
            {
                await FollowupAsync(embed: "player not found".ToWarnEmbed("Not Found !", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            if (!player_.IsAuth)
            {
                await FollowupAsync(embed: "the player is a guest".ToWarnEmbed("guest !", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            var chest = await Dynastio[provider].GetUserPersonalchestAsync(player_.Id).TryGet();
            if (chest == null)
            {
                await FollowupAsync(embed: "chest not found, join the game and put something to your chest.".ToWarnEmbed("not found", Context.Client.CurrentUser.GetAvatarUrl()));
                return;
            }
            var image = GraphicService.GetPersonalChest(chest);
            await FollowupWithFileAsync(image, "chest.jpeg", $"player chest server: `{player_.Parent.Label}` player nickname: `{player_.Nickname}`");
        }



    }
}
