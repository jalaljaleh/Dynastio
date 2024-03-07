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

            bool IsXpIncreaseable(RankingSettings settings, GuildProfile uProfile, string messageContent)
            {
                if (string.IsNullOrEmpty(messageContent)) return false;
                if (messageContent.Length < 10) return false;

                var messagesDelay = DateTime.UtcNow - uProfile.LastMessageTimestamp;
                return messagesDelay.TotalSeconds > settings.MessageDelay;
            }
            if (IsXpIncreaseable(guild.RankingSettings, uProfile, message.CleanContent))
            {
                int messageXp = guild.RankingSettings.XpPerMessage;

                if (discordUser is not { PremiumSince: null })
                    messageXp += guild.RankingSettings.XpBoosters;

                messageXp = new Random().Next(messageXp - guild.RankingSettings.XpRandom, messageXp + guild.RankingSettings.XpRandom);

                await AddXpAsync(guild, user, discordUser, txtChannel.Guild, uProfile, messageXp,txtChannel);
            }
        }
        public async Task<bool> AddXpAsync(Guild guild, User user, IUser dUser, IGuild dGuild, GuildProfile sProfile, int count,ITextChannel channel)
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
                    await AnnouncementUserLevelUpAsync(guild, sProfile, dUser, dGuild, channel).TryAsync();
                    await RequestGameLevelUpRewards(guild, user, sProfile).TryAsync();
                    await RequestRoleLevelUpRewards(guild, dUser as IGuildUser, sProfile.Level).TryAsync();
                }
                catch { }
            }
            var updated = await UpdateUserAsync(user, isLevelUpPossible);
            return updated;
        }
        public async Task DisableGuildRankingModuleAsync(Guild guild)
        {
            guild.RankingSettings.IsEnabled = false;
            await _db.UpdateAsync(guild);
        }
        public async Task<bool> RequestRoleLevelUpRewards(Guild guild, IGuildUser user, int level)
        {
            if (guild.RankingSettings.IsEnabled is false || user is null)
                return false;

            var roles = user.Guild.Roles
                                .Where(x => x.Name.StartsWith(guild.RankingSettings.RolesPrefix + " "))
                                .OrderBy(a => a.Position)
                                .Select(a => a.Id)
                                .ToList();


            var reached = roles.Where(a => user.RoleIds.Contains(a));

            // Rules are not created or not match with the prefix
            if (roles is null || roles.Count == 0)
            {
                await DisableGuildRankingModuleAsync(guild);
                return false;
            }
            // end of roles
            if (level >= roles.Count)
                return false;

            var toAdd = roles.GetRange(0, level);
            toAdd.RemoveRange(0, reached.Count());

            var result = await user.AddRolesAsync(toAdd).TryAsync();

            // role permission required
            if (result)
            {
                await DisableGuildRankingModuleAsync(guild);
                return false;
            }
            return true;
        }
        public async Task<bool> RequestGameLevelUpRewards(Guild guild, User user, GuildProfile sProfile)
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
        public async Task<bool> AnnouncementUserLevelUpAsync(Guild bGuild, GuildProfile sProfile, IUser dUser, IGuild dGuild, ITextChannel sourceChannel, bool tryMode = true)
        {
            if (bGuild.RankingSettings.LogChannelId == 0) return false;

            if (string.IsNullOrEmpty(bGuild.RankingSettings.WebhookUrl))
            {
                // permission problems
                try
                {
                    var channel = await dGuild.GetTextChannelAsync(bGuild.RankingSettings.LogChannelId);

                    var targetWebhook = await WebhookService.GetWebhookAsync(channel);

                    bGuild.RankingSettings.WebhookUrl = WebhookService.GetWebhookUrl(targetWebhook);
                    await _db.UpdateAsync(bGuild);
                }
                catch
                {
                    //  Announce the server admin
                    bGuild.RankingSettings.LogChannelId = 0;
                    await _db.UpdateAsync(bGuild);
                    return false;
                }
            }
            var content = $"You just got new level, you are level **{sProfile.Level}** and **{sProfile.Xp.Metric()}** xp  !";

            if (bGuild.RankingSettings.IsGameRewardEnabled)
                content += $"\n\nIngame Reward: You got **{DynastioApiHelper.GetLevelCoinsReward(sProfile.Level)} coins** for your reward.";
            else
                content += $"\n\nIngame Reward: Coin rewards is not supported in this server.";

            try
            {
                if(tryMode is false)
                await sourceChannel.SendMessageAsync(text: dUser.Mention,
                                    embeds: new Embed[]{ new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder(){ Name = dUser.Username, IconUrl = dUser.TryGetAvatarUrl()},
                                        Title = $"🎉 You just got new level **{sProfile.Level}**  !",
                                        Description =content,
                                        Color = Color.Green,
                                        ThumbnailUrl = dUser.TryGetAvatarUrl(),
                                    }.Build() });

                var webhook = new DiscordWebhookClient(bGuild.RankingSettings.WebhookUrl);

                await webhook.SendMessageAsync(
                                    text: dUser.Mention,
                                    username: dGuild.Name + "'s Ranking",
                                    avatarUrl: _discord.CurrentUser.TryGetAvatarUrl(),
                                    embeds: new Embed[]{ new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder(){ Name = dUser.Username, IconUrl = dUser.TryGetAvatarUrl()},
                                        Title = $"🎉 You just got new level **{sProfile.Level}**  !",
                                        Description =content,
                                        Color = Color.Green,
                                        ThumbnailUrl = dUser.TryGetAvatarUrl(),
                                    }.Build() });
            }
            catch (Exception e)
            {
                // Should use this ?!  /Consider network or discord api downs & ratelimit
                //// if (e.InnerException.Equals("Could not find a webhook with the supplied credentials."))
                //  Announce the server admin

                bGuild.RankingSettings.WebhookUrl = null;

                if (tryMode)
                    return await AnnouncementUserLevelUpAsync(bGuild, sProfile, dUser, dGuild,sourceChannel, false);

                await _db.UpdateAsync(bGuild);
                return false;
            }

            return true;
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
            if (lvl is 0)
                return _getMax(lvl + 1);

            return _getMax(lvl);

            int _getMax(int _lvl) => _lvl * 510 * (int)Math.Pow(_lvl + 1, 1.2);
        }
    }
}
