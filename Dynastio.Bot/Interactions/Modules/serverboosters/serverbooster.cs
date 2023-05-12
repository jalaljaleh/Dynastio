using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Modules.Owner;
using static Dynastio.Bot.Data.Guild;
using Dynastio.Bot.Data;


namespace Dynastio.Bot.Interactions.Modules.ServerBooster
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireServerBooster()]
    [RateLimit(60, 1, RateLimit.RateLimitType.User)]
    public class ServerBoosterModule : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }
        public IDynastioBotDatabase _db { get; set; }

        [RequireGuildOfficial]
        [SlashCommand("boost-gift", "get your redeem code")]
        public async Task server_booster_gift()
        {
            await DeferAsync(false);
            
            var message = await FollowUpToLoading(this["accounts.sync-roles.checking.title"]);

            if ((Context.User as IGuildUser).PremiumSince is null)
            {
                await FollowupAsync("you are not a server booster !");
                return;
            }
            if ((DateTime.UtcNow - (Context.User as IGuildUser).PremiumSince.Value).TotalDays > 5)
            {
                if ((DateTime.UtcNow - Context.BotUser.LastBoostGift).TotalDays > 30)
                {
                    var result = await _db.GetRedeemCodeAsync(RedeemCode.RedeemType.BoostServer);
                    if (result is not null)
                    {
                        Context.BotUser.LastBoostGift = DateTime.UtcNow;
                        await _userService.UpdateAsync(Context.BotUser);
                        try
                        {
                            await Context.User.SendMessageAsync($"```{result.Code}```");
                            await FollowupAsync($"Your redeem code sent to your DM.");
                        }
                        catch
                        {
                            await FollowupAsync($"```{result.Code}```", ephemeral: true);
                        }
                        await _db.DeleteAsync(result);
                        return;
                    }
                    await FollowupAsync($"No any more redeem code found, only 15 redeem codes are available each month.");
                    return;
                }
                await FollowupAsync($"You reached your code for this month.");
                return;
            }
            await FollowupAsync($"You can request after 5 days of boosting !.");

        }
    }
}
