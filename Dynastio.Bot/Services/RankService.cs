using Discord;
using Discord.WebSocket;
using Dynastio.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class RankService
    {
        private readonly DynastioData _dynastioData;
        private readonly GuildService _guildService;
        private readonly UserService _userService;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        public RankService(IServiceProvider services)
        {
            this._discord = services.GetRequiredService<DiscordSocketClient>();
            this._dynastioData = services.GetRequiredService<DynastioData>();
            this._guildService = services.GetRequiredService<GuildService>();
            this._userService = services.GetRequiredService<UserService>();
            this._services = services;
        }

        public const int _nextScoreTime = 60;
        public const int _updateUserTime = 240;
        public static int[] _randomScore = { 40, 80 };
        public static int[] _randomScoreServerBooster = { 50, 100 };
        public static int getMax(int lvl)
        {
            return (((lvl + 250) * (int)Math.Pow(lvl + 1, 2.1))) + 3600;
        }
        private ulong[] _score_channels = {
            480966712318099487, //
            486591124836974592, //
            1098632452274135112,//
            1098918867255967814,//
            1098248723013841026,//
            1098608343947415575,//
            1098263349873082438,//
        };
        public async Task<(bool xpResult, bool levelupResult, User user, IGuildUser discordUser)> TryAddMessageXpAsync(IUserMessage message)
        {
            if (message.Channel is null) return (false, false, null, null);
            if (_score_channels.Contains(message.Channel.Id) is false)
                return (false, false, null, null);

            
            var user = await _dynastioData.GetUserAsync(message.Author.Id);
            var discordUser = message.Author as IGuildUser;

            if (IsXpIncreaseable(user))
            {
                int messageXp = GetMessageXp(discordUser);
                IncreaseUserXp(user, messageXp);


                var levelupResult = TryLevelUpUser(user);
                var updated = await UpdateUserAsync(user, levelupResult);
                if (levelupResult)
                {
                    // should impelement game reward here ?!!
                }
                return (true, levelupResult, user, discordUser);
            }
            return (false, false, null, null);
        }
        public bool IsLevelIncreaseable(long xp, int level, out int max)
        {
            max = getMax(level);
            return xp > max;
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
        public int GetMessageXp(IGuildUser user)
        {
            var isServerBooster = user is not { PremiumSince: null };
            int[] score = isServerBooster ? _randomScoreServerBooster : _randomScore;

            return Global.Main.Random.Next(score[0], score[1]);
        }
        public bool IsXpIncreaseable(User user)
        {
            var last_activiy_score_time = DateTime.UtcNow - user.last_activiy_score_time;
            return last_activiy_score_time.TotalSeconds > _nextScoreTime;
        }
        public void IncreaseUserXp(User user, int xp)
        {
            user.activiy_score = user.activiy_score + xp;
            user.last_activiy_score_time = DateTime.UtcNow;
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
