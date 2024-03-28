using Discord;
using Discord.Rest;
using Discord.Webhook;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Dynastio.Net;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class RankingService : ServicesBase
    {
        private readonly UserService _userService;
        private readonly DynastioApi _dynastioApi;

        public RankingService(IServiceProvider services) : base(services)
        {
            _userService = services.GetRequiredService<UserService>();
            _dynastioApi = services.GetRequiredService<DynastioApi>();
        }

        public async Task TryAddMessageXpAsync(Guild guild, User user, IUserMessage message)
        {
            if (guild.RankingSettings.IsEnabled is false) return;

            if (message.Channel is null || message.Channel is not ITextChannel txtChannel) return;

            if (guild.RankingSettings.IsLevelUpChannel(txtChannel.Id) is false) return;

            var discordUser = message.Author as IGuildUser;
            var uProfile = user.GetRankingProfile(guild.Id);


            if (IsXpIncreaseable(guild.RankingSettings, uProfile, message.CleanContent))
            {
                int messageXp = guild.RankingSettings.XpPerMessage;

                if (discordUser is not { PremiumSince: null })
                    messageXp += guild.RankingSettings.XpBoosters;

                messageXp = new Random().Next(messageXp - guild.RankingSettings.XpRandom, messageXp + guild.RankingSettings.XpRandom);

                await AddXpAsync(guild, user, discordUser, txtChannel.Guild, uProfile, messageXp, txtChannel);
            }
        }
        bool IsXpIncreaseable(RankingSettings settings, GuildProfile uProfile, string messageContent)
        {
            if (string.IsNullOrEmpty(messageContent)) return false;
            if (messageContent.Length < 10) return false;

            var messagesDelay = DateTime.UtcNow - uProfile.LastMessageTimestamp;
            return messagesDelay.TotalSeconds > settings.Delay;
        }
        public async Task<bool> AddXpAsync(Guild guild, User user, IUser dUser, IGuild dGuild, GuildProfile sProfile, int count, ITextChannel channel)
        {
            sProfile.LastMessageTimestamp = DateTime.UtcNow;
            sProfile.Xp += count;

            var currentLevelXpRequirement = GetCurrentLevelXpRequirement(sProfile.Level);

            var isLevelUpPossible = sProfile.Xp > currentLevelXpRequirement;
            if (isLevelUpPossible)
            {
                sProfile.Xp = sProfile.Xp - currentLevelXpRequirement;
                sProfile.Level++;

                try
                {
                    await NotifyUserLevelUpAsync(user, guild, sProfile, dUser, dGuild, channel).TryAsync();
                    await SynchronizeGameUser(guild, user, sProfile).TryAsync();
                    await SynchronizeUserRolesAsync(guild, dUser as IGuildUser, sProfile.Level).TryAsync();
                }
                catch { }
            }
            var updated = await UpdateUserAsync(user, isLevelUpPossible);
            return updated;
        }
        public async Task SetUnqualifiedGuildAsync(Guild guild)
        {
            guild.RankingSettings.IsEnabled = false;
            await _db.UpdateAsync(guild);
        }

        public IEnumerable<IRole> GetGuildRankingRoles(IGuild guild, string rolePrefix)
        {
            return guild.Roles
                                .Where(x => x.Name.StartsWith(rolePrefix + " "))
                                .OrderBy(a => a.Position);
        }
        public IEnumerable<IRole> GetUserRankingRoles(IGuildUser user, IEnumerable<IRole> serverRankingRoles)
        {
            return serverRankingRoles.Where(a => user.RoleIds.Contains(a.Id));
        }
        public async Task<bool> SynchronizeUserRolesAsync(Guild guild, IGuildUser user, int level)
        {
            if (guild.RankingSettings.IsEnabled is false) return false;

            var serverRoles = GetGuildRankingRoles(user.Guild, guild.RankingSettings.RolesPrefix).ToList();
            if (serverRoles?.Any() == false)
            {
                return false;
            }

            var userRoles = GetUserRankingRoles(user, serverRoles).ToList();

            // Everything is okay
            if (serverRoles.Count == userRoles.Count)
                return true;

            // when roles are few
            if (serverRoles.Count < level)
                level = serverRoles.Count;

            var toAdd = serverRoles.GetRange(userRoles.Count - 1 < 0 ? 0 : userRoles.Count - 1, (level - userRoles.Count) + 1);

            var result = await user.AddRolesAsync(toAdd)
                .TryAsync();

            // role permission required
            if (result is false)
            {
                await SetUnqualifiedGuildAsync(guild);
                return false;
            }
            return true;
        }
        public async Task<bool> SynchronizeGameUser(Guild guild, User user, GuildProfile sProfile)
        {
            if (guild.RankingSettings.IsGameRewardEnabled)
            {
                bool isGameAccountConnected = !string.IsNullOrEmpty(user.gameAccountId);
                if (isGameAccountConnected)
                {
                    await _dynastioApi.UpdateDiscordRank(user.gameAccountId, sProfile.Level);
                    return true;
                }
                return false;
            }
            return false;
        }
        public async Task<bool> NotifyUserLevelUpAsync(User user, Guild bGuild, GuildProfile sProfile, IUser dUser, IGuild dGuild, ITextChannel sourceChannel)
        {
            ITextChannel loggChannel = null;
            if (bGuild.RankingSettings.LogChannelId != 0)
            {
                loggChannel = await dGuild.GetTextChannelAsync(bGuild.RankingSettings.LogChannelId);
                if (loggChannel is null)
                {
                    await SetUnqualifiedGuildAsync(bGuild);
                    return false;
                }
            }

            var embed = string.Format(
                " You just got new level, you are level **{0}** and **{1}** xp  !{2}",

                sProfile.Level,
                sProfile.Xp.Metric(),

                (bGuild.RankingSettings.IsGameRewardEnabled
                ? $"\n\n Ingame Reward: You got **{DynastioApiHelper.GetLevelCoinsReward(sProfile.Level)} coins** for your reward." + (

                user.IsAccountConnected
                  ? $"\n\n✨ Note: Your reward has been added to your account **{user.GetAccountService()}** directly !"
                  : "\n\n⚠️ **Note:** Connect your game account to get the reward. ⚠️")


                : $"\n\nIngame Reward: Coin rewards is not supported in this server.")
                    ).ToEmbed(
                title: $"🎉 You just got new level **{sProfile.Level}**  !🎉",
                thumbnailUrl: Global.Resource.RewardImageUrl,
                color: (user.IsAccountConnected & bGuild.RankingSettings.IsGameRewardEnabled) ? Color.Green : Color.Red);


            await sourceChannel.SendMessageAsync(dUser.Mention, embed: embed).TryAsync();


            var result = await loggChannel.SendMessageAsync(dUser.Mention, embed: embed).TryAsync();

            if (result.isSuccesful is false)
                await SetUnqualifiedGuildAsync(bGuild);

            return result.isSuccesful;
        }
        public async Task<bool> UpdateUserAsync(User user, bool force = false)
        {
            var lastUpdate = DateTime.UtcNow - user.LastUpdateTime;
            if (force || lastUpdate.TotalSeconds > 240)
            {
                return await _userService.UpdateUserAsync(user);
            }
            return false;
        }
        public static int GetLevelUpRequirementXp(GuildProfile sUser)
        {
            return (int)(GetCurrentLevelXpRequirement(sUser.Level) - sUser.Xp);
        }
        public static int GetCurrentLevelXpRequirement(int lvl)
        {
            return ((lvl + 1) * 510) * (int)Math.Pow(lvl + 1, 1.2);
        }
    }
}
