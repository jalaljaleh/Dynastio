using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using static Dynastio.Bot.Data.Guild;
using Dynastio.Bot.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Dynastio.Bot.Interactions.commands
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireServerBooster()]
    [RateLimit(60, 1, RateLimit.RateLimitType.User)]
    public class ServerBoosterModule : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public IDynastioBotDatabase _db { get; set; }

        [RequireGuildOfficial]
        [SlashCommand("gift", "get your redeem code", false, RunMode.Sync)]
        public async Task server_booster_gift()
        {
            await DeferAsync(false);

            var message = await FollowUpToLoading(this["accounts.sync-roles.checking.title"]);


            if ((Context.User as IGuildUser).PremiumSince is null)
            {
                await message.ModifyAsync(x => x.Embed = "Your are not server booster.".ToEmbed());
                return;
            }

            if ((DateTime.UtcNow - (Context.User as IGuildUser).PremiumSince.Value).TotalDays < 15)
            {
                await message.ModifyAsync(x => x.Embed = $"You can request after 15 days of boosting !.".ToEmbed());
                return;
            }

            if ((DateTime.UtcNow - Context.BotUser.LastBoostGift).TotalDays < 30)
            {
                await message.ModifyAsync(x => x.Embed = $"You reached the code for this month already.".ToEmbed());
                return;
            }

            var result = await _db.GetRedeemCodeAsync(RedeemCode.RedeemType.Boost_Server);
            if (result is null)
            {
                await message.ModifyAsync(x => x.Embed = $"No any more redeem code found, only 15 redeem codes are available each month.".ToEmbed());
                return;
            }


            var sendMessageResult = await Context.User.SendMessageAsync(
                $"# Server Booster Redeem Code\n" +
                $"```{result.Code}```").TryAsync();

            if (sendMessageResult.isSuccesful)
                await message.ModifyAsync(x => x.Embed = $"Your redeem code sent to your DM.".ToEmbed());
            else
                await FollowupAsync($"```{result.Code}```", ephemeral: true);


            Context.BotUser.LastBoostGift = DateTime.UtcNow;

            await _userService.UpdateAsync(Context.BotUser);

            await _db.DeleteAsync(result);

            await _discord.GetGuild(GuildService._officialGuildId)
                    .GetTextChannel(RankService._scoreChannelId)
                    .SendMessageAsync(
                    text: userMention,
                    embed: new EmbedBuilder()
                    {
                        Title = $"🎉 You just got Server Booster redeem code!",
                        Description = $"You got **Server Booster** redeem code for ` boosting the server. `",
                        Color = Color.Green,
                        ThumbnailUrl =
                        "https://cdn.discordapp.com/attachments/1111209352095871028/1111209352217509938/openiron.png",
                    }.Build());

        }
    }
}
