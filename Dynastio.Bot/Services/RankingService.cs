using Discord;
using Discord.Rest;
using Discord.Webhook;
using Dynastio.Bot.Database;
using Dynastio.Net;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class RankingService : ServicesBase
    {
        private readonly UserService _userService;

        public RankingService(IServiceProvider services) : base(services)
        {
            _userService = services.GetRequiredService<UserService>();
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
                return messagesDelay.TotalSeconds > settings.Delay;
            }
            if (IsXpIncreaseable(guild.RankingSettings, uProfile, message.CleanContent))
            {
                int messageXp = guild.RankingSettings.XpPerMessage;

                if (discordUser is not { PremiumSince: null })
                    messageXp += guild.RankingSettings.XpBoosters;

                messageXp = new Random().Next(messageXp - guild.RankingSettings.XpRandom, messageXp + guild.RankingSettings.XpRandom);

                await AddXpAsync(guild, user, discordUser, txtChannel.Guild, uProfile, messageXp);
            }
        }
        public async Task<bool> AddXpAsync(Guild guild, User user, IUser dUser, IGuild dGuild, GuildProfile sProfile, int count)
        {
            sProfile.LastMessageTimestamp = DateTime.UtcNow;
            sProfile.Xp += count;

            var currentLevelXpRequirement = GetCurrentLevelXpRequirement(sProfile.Level);

            var isLevelUpAvailable = sProfile.Xp > currentLevelXpRequirement;
            if (isLevelUpAvailable)
            {
                sProfile.Xp = sProfile.Xp - currentLevelXpRequirement;
                sProfile.Level++;

                try
                {
                    await AnnouncementUserLevelUpAsync(guild, sProfile, dUser, dGuild);
                }
                catch { }
            }
            var updated = await UpdateUserAsync(user, isLevelUpAvailable);
            return updated;
        }
        public async Task<bool> AnnouncementUserLevelUpAsync(Guild bGuild, GuildProfile sProfile, IUser dUser, IGuild dGuild, bool tryMode = true)
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

            try
            {
                var webhook = new DiscordWebhookClient(bGuild.RankingSettings.WebhookUrl);

                await webhook.SendMessageAsync(
                                    text: dUser.Mention,
                                    username: dGuild.Name + "'s Ranking",
                                    avatarUrl: _discord.CurrentUser.TryGetAvatarUrl(),
                                    embeds: new Embed[]{ new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder(){ Name = dUser.Username, IconUrl = dUser.TryGetAvatarUrl()},
                                        Title = $"🎉 You just got new level **{sProfile.Level}**  !",
                                        Description = $"You just got new level, you are level **{sProfile.Level}** and {sProfile.Xp} xp  !",
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
                   return await AnnouncementUserLevelUpAsync(bGuild, sProfile, dUser, dGuild, false);
                
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
