using Discord;
using Discord.WebSocket;
using Dynastio.Data;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class RankService
    {
        private readonly DynastioData _dynastioData;
        private readonly DynastioClient _dynastioClient;
        private readonly GuildService _guildService;
        private readonly UserService _userService;
        private readonly DiscordSocketClient _discord;
        private readonly WebhookService _webhook;
        private readonly IServiceProvider _services;
        public RankService(IServiceProvider services)
        {
            this._discord = services.GetRequiredService<DiscordSocketClient>();
            this._dynastioData = services.GetRequiredService<DynastioData>();
            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._guildService = services.GetRequiredService<GuildService>();
            this._userService = services.GetRequiredService<UserService>();
            this._webhook = services.GetRequiredService<WebhookService>();
            this._services = services;
        }

        public const int _nextScoreTime = 60;
        public const int _updateUserTime = 240;
        public const int maxLevel = 40;
        public const double maxReward = 10000;
        public const int _score = 30;
        public const int _boostersExpandableXp = 15;
        public const int _randomXp = 10;
        private ulong[] _score_channels = {
            480966712318099487, //
            486591124836974592, //
            1098632452274135112,//
            1098918867255967814,//
            1098248723013841026,//
            1098608343947415575,//
            1098263349873082438,//
        };
        public static int getMax(int lvl)
        {
            if (lvl is 0)
                return _getMax(lvl);

            return _getMax(lvl) + 10000;

            int _getMax(int _lvl) => (_lvl < 21 ? 900 : 600) * (int)Math.Pow(_lvl + 1, 2.1);
        }
        public static int RequiredXpToLevelUp(User user)
        {
            return (int)(getMax(user.activiy_level) - user.activiy_score);
        }
        public static double CalculateLevelReward(int level)
        {
            double b = 1.0 / maxLevel;

            double a = maxReward / (Math.Exp(1) - 1);

            return Math.Round(a * (Math.Exp(b * level) - 1));
        }
        public async Task<(bool xpResult, bool levelupResult, User user, IGuildUser discordUser)> TryAddMessageXpAsync(IUserMessage message)
        {
            if (message.Channel is null) return (false, false, null, null);
            if (_score_channels.Contains(message.Channel.Id) is false)
                return (false, false, null, null);

            var user = await _dynastioData.GetUserAsync(message.Author.Id);
            var discordUser = message.Author as IGuildUser;

            if (IsXpIncreaseable(discordUser, user, message.CleanContent))
            {
                int messageXp = GetMessageXp(discordUser, user);
                IncreaseUserXp(user, messageXp);

                var levelupResult = TryLevelUpUser(user);
                var updated = await UpdateUserAsync(user, levelupResult);

                if (levelupResult)
                {
                    bool isGameAccountConnected = string.IsNullOrEmpty(user.gameAccountId);

                    if (isGameAccountConnected)
                    {
                        // remove when game updated !
                        if (false)
                            await _dynastioClient.UpdateDiscordRank(user.gameAccountId, user.activiy_level);
                    }

                    var role = _userService.GetHighestRankedRoleUser(discordUser);

                    await _webhook.LogRewardAsync(discordUser.Mention, embeds: new List<Embed>(){ new EmbedBuilder()
                            {
                                Title = "New Level Unlocked",
                                Description = $"🎉 You just unlocked new level **{user.activiy_level}**, level reward unlocked !",
                                Color = isGameAccountConnected ? (role?.Color ?? Color.Orange) : Color.Red,
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    .WithName("Unlocked Rewards")
                                    .WithValue(isGameAccountConnected ? $"You just got **{CalculateLevelReward(user.activiy_level)}** coins !":$"You will receive your rewards when you have connected your game account, use `/accounts connect` command.")
                                    .WithIsInline(true),
                                },
                                ThumbnailUrl =  "https://cdn.discordapp.com/attachments/1111209352095871028/1111209352217509938/openiron.png"
                            }.Build() });

                }
                return (true, levelupResult, user, discordUser);
            }
            return (false, false, null, null);
        }
        public static int GetReachableMessageXp(IGuildUser user, User buser)
        {
            var additiveXp = GetAdditiveXp(user, buser);
            return _score + additiveXp;
        }
        public int GetMessageXp(IGuildUser user, User buser)
        {
            var xp = GetReachableMessageXp(user, buser);
            return Global.Main.Random.Next(xp - _randomXp, xp + _randomXp);
        }
        public static int GetAdditiveXp(IGuildUser user, User buser)
        {
            var xp = GetServerBoosterXp(user);
            return xp + (int)buser.activiy_score_additive;
        }
        public static int GetServerBoosterXp(IGuildUser user)
        {
            var isServerBooster = user is not { PremiumSince: null };
            return isServerBooster ? _boostersExpandableXp : 0;
        }
        public bool IsXpIncreaseable(IGuildUser discordUser, User user, string messageContent)
        {
            if (string.IsNullOrEmpty(messageContent)) return false;
            if (!HasXpRequirements(messageContent)) return false;
            if (!HasXpRequirements(discordUser)) return false;

            var last_activiy_score_time = DateTime.UtcNow - user.last_activiy_score_time;
            return last_activiy_score_time.TotalSeconds > _nextScoreTime;
        }
        public static bool HasXpRequirements(IGuildUser user)
        {
            return user.CreatedAt.Month < 3 ? false : true;
        }
        public static bool HasXpRequirements(string messageContent)
        {
            return messageContent.Length < 10 ? false : true;
        }
        public bool IsLevelIncreaseable(long xp, int level, out int max)
        {
            max = getMax(level);
            return xp > max;
        }
        public void IncreaseUserXp(User user, int xp)
        {
            user.activiy_score = user.activiy_score + xp;
            user.last_activiy_score_time = DateTime.UtcNow;
        }
        public bool TryLevelUpUser(User _user)
        {
            if (IsLevelIncreaseable(_user.activiy_score, _user.activiy_level, out int max))
            {
                _user.activiy_score = _user.activiy_score - max;
                _user.activiy_level++;
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateUserAsync(User user, bool force = false)
        {
            var lastUpdate = DateTime.UtcNow - user.last_update;
            if (force || lastUpdate.TotalSeconds > _updateUserTime)
            {
                return await _dynastioData.UpdateAsync(user);
            }
            return false;
        }


    }

}
