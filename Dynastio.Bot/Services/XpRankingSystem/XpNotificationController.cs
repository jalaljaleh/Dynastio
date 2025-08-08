using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;

namespace Dynastio.Bot.Services.XpRankingSystem
{
    public static class XpNotificationController
    {
        //public static async Task<bool> NotifyUserLevelUpAsync(User user, Guild guild, UserGuildProfile profile, IUser discordUser, IGuild discordGuild, ITextChannel sourceChannel, Func<Guild, IGuild, Task> disableGuildCallback)
        //{
        //    ITextChannel logChannel = null;

        //    if (guild.XpSystemSettings.RankingLoggerChannelId != 0)
        //    {
        //        logChannel = await discordGuild.GetTextChannelAsync(guild.XpSystemSettings.RankingLoggerChannelId);
        //        if (logChannel == null)
        //        {
        //            await disableGuildCallback(guild, discordGuild);
        //            return false;
        //        }
        //    }

        //    var embed = string.Format(
        //        "You just got new level, you are level **{0}** and **{1}** xp!{2}",
        //        profile.Level,
        //        profile.Xp.Metric(),
        //        guild.XpSystemSettings.IsGameRewardEnabled
        //            ? $"\n\nIngame Reward: You got **{XpCalculator.GetLevelCoinsReward(profile.Level)} coins**." +
        //              (user.IsAccountConnected
        //                  ? $"\n\n✨ Note: Your reward has been added to your account **{user.GetAccountService()}**!"
        //                  : "\n\n⚠️ Connect your game account to get the reward.")
        //            : "\n\nIngame Reward: Coin rewards are not supported in this server.")
        //        .ToEmbed(
        //            title: $"🎉 You just got new level **{profile.Level}**! 🎉",
        //            thumbnailUrl: GlobalResource.RewardImageUrl,
        //            color: (user.IsAccountConnected && guild.XpSystemSettings.IsGameRewardEnabled) ? Color.Green : Color.Red);

        //    var message = await sourceChannel.SendMessageAsync(discordUser.Mention, embed: embed,
        //        components: user.IsAccountConnected ? null : new ComponentBuilder()
        //            .WithButton(ConnectAccountButton.GetButton(_global.GetOrDefault()))
        //            .Build()).TryAsync();

        //    await Task.Delay(100);

        //    var result = await logChannel.SendMessageAsync(discordUser.Mention + " " + message.result.GetJumpUrl(), embed: embed).TryAsync();

        //    if (!result.isSuccesful)
        //        await disableGuildCallback(guild, discordGuild);

        //    return result.isSuccesful;
        //}
    }
}