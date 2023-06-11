using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using System.ComponentModel;
using Dynastio.Bot.Data;
using Dynastio.Bot.Interactions.AutoCompeletes;

namespace Dynastio.Bot.Interactions.modules.dynastio
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RateLimit(4)]
    public class ProfileModule : CustomInteractionModuleBase
    {
        public DynastioClient _dynastio { get; set; }
        public GraphicService _graphicService { get; set; }

        [RequireUserDynastioAccount]
        [SlashCommand("profile", "your dynastio profile")]
        public async Task profile([MaxLength(20), Autocomplete(typeof(AutoCompeleteAccounts))] string account = "")
        {
            await DeferAsync();

            UserAccount account_ = string.IsNullOrWhiteSpace(account)
                    ? Context.BotUser.GetDefaultAccount()
                    : Context.BotUser.GetAccountByHashCode(account);

            var profileCard = await _dynastio.GetUserProfileCardAsync(account_.Id)
                .TryAsync();


            var image = await _graphicService.GetPersonalChestAsync(Context.User as IGuildUser, account_, profileCard.result);

            await DiscordStream.FollowupWithFileAsync(Context, image, $"profile-card-{Context.User.Id}.png", $"");
        }


    }
}
