using Discord;
using Discord.Rest;
using Discord.Webhook;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Bot
{
    public class RankingService
    {
        private readonly DynastioApi _dynastioApi;
        private readonly IServiceProvider _services;
        public RankingService(IServiceProvider services)
        {
            _services = services;
            _dynastioApi = services.GetRequiredService<DynastioApi>();
        }
        public async Task<List<IRole>> SyncDiscordRolesAsync(Guild guild, IGuildUser discordUser, GuildProgress profile)
        {
            if (guild.RankingSettings.IsRankingRoleAssignmentEnabled)
            {
                var result_ = await RankingServiceHelper.AssignmentUserRolesAsync(guild, discordUser, profile.Level).TryAsync();
                return result_.result;
            }
            return default;
        }
        public async Task TryAddMessageXpAsync(Guild guild, User user, IUserMessage message)
        {
            if (!guild.RankingSettings.IsEnabled || message.Channel is not ITextChannel channel || !guild.RankingSettings.IsAllowedChannel(channel.Id))
                return;

            var discordUser = message.Author as IGuildUser;
            var profile = user.GetOrCreateGuildProfile(guild.Id);

            if (!IsXpIncreaseable(guild.RankingSettings, profile, message.CleanContent)) return;

            int baseXp = guild.RankingSettings.BaseXpPerMessage;
            if (discordUser?.PremiumSince != null)
                baseXp += guild.RankingSettings.BoosterXp;

            int randomizedXp = Random.Shared.Next(baseXp - guild.RankingSettings.RandomXpBonus, baseXp + guild.RankingSettings.RandomXpBonus);
            profile.RecordMessage(DateTime.UtcNow);

            profile.Xp += randomizedXp;

            bool leveledUp = LevelUp(profile);
            if (leveledUp)
            {
                List<IRole> rolesResult = default;
                rolesResult = await SyncDiscordRolesAsync(guild, discordUser, profile);

                await RankingServiceNotificationController.NotifyUserLevelUpAsync(user, guild, profile, discordUser, channel.Guild, channel, rolesResult);

                await UpdateUserGameRewards(profile, user, guild).TryAsync();
            }

            await UpdateUserAsync(user, leveledUp);
        }

        private bool IsXpIncreaseable(RankingSettings settings, GuildProgress profile, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length < 10) return false;
            return (DateTime.UtcNow - profile.LastMessageAtUtc).TotalSeconds > settings.MessageScoreCooldownSeconds;
        }

        private bool LevelUp(GuildProgress profile)
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

        public async Task<bool> UpdateUserGameRewards(GuildProgress profile, User user, Guild guild)
        {
            if (!guild.RankingSettings.IsGameRewardEnabled || !user.HasLinkedAccount)
                return false;

            await _dynastioApi.UpdateDiscordRankAsync(user.GetRewardAccount().Id, profile.Level);
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user, bool force = false)
        {
            if (force || (DateTime.UtcNow - user.LastUpdatedUtc).TotalSeconds > 300)
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
