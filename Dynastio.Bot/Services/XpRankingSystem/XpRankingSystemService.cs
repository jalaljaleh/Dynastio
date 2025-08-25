using Discord;
using Discord.Rest;
using Discord.Webhook;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services.XpRankingSystem;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class XpRankingSystemService
    {
        private readonly DynastioApi _dynastioApi;
        private readonly IServiceProvider _services;
        public XpRankingSystemService(IServiceProvider services)
        {
            _services = services;
            _dynastioApi = services.GetRequiredService<DynastioApi>();
        }

        public async Task TryAddMessageXpAsync(Guild guild, User user, IUserMessage message)
        {
            if (!guild.XpSystemSettings.IsEnabled || message.Channel is not ITextChannel channel || !guild.XpSystemSettings.IsXpChannel(channel.Id))
                return;

            var discordUser = message.Author as IGuildUser;
            var profile = user.GetGuildProfile(guild.Id);

            if (!IsXpIncreaseable(guild.XpSystemSettings, profile, message.CleanContent)) return;

            int baseXp = guild.XpSystemSettings.XpPerMessage;
            if (discordUser?.PremiumSince != null)
                baseXp += guild.XpSystemSettings.XpBoosters;

            int randomizedXp = Random.Shared.Next(baseXp - guild.XpSystemSettings.XpRandom, baseXp + guild.XpSystemSettings.XpRandom);
            profile.LastMessageTimestamp = DateTime.UtcNow;
            profile.Xp += randomizedXp;

            bool leveledUp = LevelUp(profile);
            if (leveledUp)
            {
                List<IRole> rolesResult = default;
                if (guild.XpSystemSettings.IsRankingRoleAssignmentEnabled)
                {
                    var result_ = await XpRankingSystemServiceHelper.AssignmentUserRolesAsync(guild, discordUser, profile.Level).TryAsync();
                    rolesResult = result_.result;
                }

                await XpNotificationController.NotifyUserLevelUpAsync(user, guild, profile, discordUser, channel.Guild, channel, rolesResult);

                await UpdateUserGameRewards(profile, user, guild).TryAsync();
            }

            await UpdateUserAsync(user, leveledUp);
        }

        private bool IsXpIncreaseable(XpSystemSettings settings, UserGuildProfile profile, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length < 10) return false;
            return (DateTime.UtcNow - profile.LastMessageTimestamp).TotalSeconds > settings.MessageScoreCooldown;
        }

        private bool LevelUp(UserGuildProfile profile)
        {
            //  if (XpCalculator.MaxLevel <= profile.Level) return false;

            int requiredXp = XpCalculator.GetCurrentLevelXpRequirement(profile.Level);
            if (profile.Xp > requiredXp)
            {
                profile.Xp -= requiredXp;
                profile.Level++;
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserGameRewards(UserGuildProfile profile, User user, Guild guild)
        {
            if (!guild.XpSystemSettings.IsGameRewardEnabled || !user.IsAccountConnected)
                return false;

            await _dynastioApi.UpdateDiscordRank(user.gameAccountId, profile.Level);
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user, bool force = false)
        {
            if (force || (DateTime.UtcNow - user.LastUpdateTime).TotalSeconds > 300)
            {
                return await _services.GetService<UsersService>().UpdateUserAsync(user);
            }
            return false;
        }

        //public async Task SetUnqualifiedGuildAsync(Guild guild, IGuild discordGuild)
        //{
        //    guild.XpSystemSettings.IsEnabled = false;
        //    await _db.UpdateAsync(guild);

        //    try
        //    {
        //        var owner = await discordGuild.GetOwnerAsync();
        //        await owner.SendMessageAsync("Ranking module disabled due to an error. Please re-enable it.");
        //    }
        //    catch { }
        //}
    }
}
