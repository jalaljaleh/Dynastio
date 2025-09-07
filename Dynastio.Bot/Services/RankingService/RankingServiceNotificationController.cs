using Discord;
using Dynastio.Bot.Database;
using Dynastio.Net;

namespace Dynastio.Bot
{
    public static class RankingServiceNotificationController
    {
        /// <summary>
        /// Notifies a user that they have leveled up by sending an embed
        /// to the source channel and an optional log channel.
        /// </summary>
        public static async Task<bool> NotifyUserLevelUpAsync(User user, Guild guild, GuildProgress profile, IUser discordUser, IGuild discordGuild, ITextChannel sourceChannel, IReadOnlyList<IRole> unlockedRoles)
        {
            // 1. Build the embed
            var embed = BuildLevelUpEmbed(user, guild, profile, unlockedRoles);

            // 2. Send to the channel where the XP was earned
            var sendOp = await sourceChannel
                .SendMessageAsync(discordUser.Mention, embed: embed)
                .TryAsync();

            // If we couldn't even send the first message, bail out now
            if (!sendOp.isSuccessful)
                return false;

            // 3. Resolve (and validate) the logging channel
            var logChannel = await ResolveLogChannelAsync(discordGuild, guild);
            if (logChannel == null)
                return true;

            // 4. Short pause to allow the jump link to register
            await Task.Delay(100);

            // 5. Log the same embed with a jump link back to the source
            var jumpUrl = sendOp.result.GetJumpUrl();
            var logOp = await logChannel
                .SendMessageAsync($"{discordUser.Mention} {jumpUrl}", embed: embed)
                .TryAsync();

            return logOp.isSuccessful;
        }

        /// <summary>
        /// Fetches the configured log channel or returns null.
        /// If the channel does not exist, clears the saved channel ID.
        /// </summary>
        private static async Task<ITextChannel> ResolveLogChannelAsync(
            IGuild discordGuild,
            Guild guild)
        {
            var channelId = guild.RankingSettings.RankingLogChannelId;
            if (channelId == 0)
                return null;

            var channel = await discordGuild.GetTextChannelAsync(channelId);
            if (channel == null)
                guild.RankingSettings.RankingLogChannelId = 0;

            return channel;
        }

        /// <summary>
        /// Constructs the embed shown to the user on level-up.
        /// Uses the last element of unlockedRoles (if any) for color and thumbnail.
        /// </summary>
        private static Embed BuildLevelUpEmbed(
            User user,
            Guild guild,
            GuildProgress profile,
            IReadOnlyList<IRole> unlockedRoles)
        {
            var latestRole = unlockedRoles.LastOrDefault();
            var thumbnailUrl = latestRole?.GetIconUrl() ?? GlobalResource.RewardImageUrl;
            var embedColor = latestRole?.Color ?? DetermineFallbackColor(user, guild);
            var descriptionText = BuildDescription(user, guild, profile);

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"🎉 Level {profile.Level} Unlocked! 🎉")
                .WithDescription(descriptionText)
                .WithThumbnailUrl(thumbnailUrl)
                .WithColor(embedColor);

            // Properly list out role mentions, or show "None"
            var rolesValue = unlockedRoles.Any()
                ? string.Join(", ", unlockedRoles.Select(r => r.Mention))
                : "None found";

            embedBuilder.AddField("Unlocked Roles", rolesValue, inline: false);

            return embedBuilder.Build();
        }

        /// <summary>
        /// Chooses a fallback embed color when no role icon is available.
        /// </summary>
        private static Color DetermineFallbackColor(User user, Guild guild) =>
            guild.RankingSettings.IsGameRewardEnabled && user.HasLinkedAccount
                ? Color.Green
                : Color.Red;

        /// <summary>
        /// Builds the textual part of the embed describing XP and rewards.
        /// </summary>
        private static string BuildDescription(
            User user,
            Guild guild,
            GuildProgress profile)
        {
            var xpInfo = $"Reached level **{profile.Level}** with **{profile.Xp.ToMetric()} XP**!";
            var rewardSeg = guild.RankingSettings.IsGameRewardEnabled
                ? BuildRewardInfo(user, profile.Level)
                : "In-game rewards are not supported on this server.";

            return $"{xpInfo}\n\n{rewardSeg}";
        }

        /// <summary>
        /// Describes the in-game reward or prompts account connection.
        /// </summary>
        private static string BuildRewardInfo(User user, int level)
        {
            if (!user.HasRewardAccount)
                return "⚠️ Connect your game account to claim your coins.";

            var coins = XpCalculator.GetLevelCoinsReward(level);
            var accountName = user.GetRewardAccount().ServiceName;
            return $"In-game reward: **{coins} coins** added to **{accountName}**.";
        }
    }
}