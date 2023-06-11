using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class RankService
    {
        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly DiscordSocketClient _discord;
        private readonly IDynastioBotDatabase _database;
        private readonly IServiceProvider _services;
        public RankService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _userService = services.GetRequiredService<UserService>();
            _guildService = services.GetRequiredService<GuildService>();
            _database = services.GetRequiredService<IDynastioBotDatabase>();
            _services = services;

            _discord.UserJoined += _discord_UserJoined;
        }

        private async Task _discord_UserJoined(SocketGuildUser user)
        {
            var buser = await _userService.GetUserAsync(user.Id, false);

            if (buser is not null)
                await SyncMemberRolesAsync(user, buser, null);
        }


        public static ulong _scoreChannelId = 1108998382996946964;
        public async Task AddXpAsync(User user, ulong @operator, int count, string reason = "no reason provided.")
        {
            user.activiy_score += count;
            await _userService.UpdateAsync(user);

            var targetUser = _discord.GetUser(user.Id);

            await _discord.GetGuild(GuildService._officialGuildId)
                .GetTextChannel(_scoreChannelId)
                .SendMessageAsync(
                text: user.Id.ToUserMention(),
                embed: new EmbedBuilder()
                {
                    Title = $"🎉 You just got {count} xp !",
                    Description = $"You got **{count}** xp for ` {reason} `",
                    Color = Color.Green,
                    ThumbnailUrl =
                    targetUser?.GetAvatarUrl() ??
                    targetUser?.GetDefaultAvatarUrl() ??
                    "https://cdn.discordapp.com/attachments/1098332386674085988/1098521187191095387/dynastio.png"
                }.Build());
        }

        public async Task SyncMemberRolesAsync(IGuildUser duser, User buser, ITextChannel channel)
        {
            var rankedRoles = duser.Guild.Roles
                .Where(x => x.Name.StartsWith("rank: "))
                .OrderBy(a => a.Position)
                .Select(a => a.Id)
                .ToList();

            var userRankedroles = duser.RoleIds.Where(a => rankedRoles.Contains(a));

            var rolesToAdd = rankedRoles.GetRange(0, buser.activiy_level);
            rolesToAdd.RemoveRange(0, userRankedroles.Count());

            await duser.AddRolesAsync(rolesToAdd);

            var latestRole = duser.Guild.Roles.First(a => a.Id == rolesToAdd.Last());

            if (channel is null) return;

            await channel.SendMessageAsync(buser.Id.ToUserMention(),
                embed: new EmbedBuilder()
                {
                    Title = " You just got new level 🎉",
                    Description = $"🎉 You just got new level **{buser.activiy_level}** exp: **{buser.activiy_score}**",
                    Color = latestRole?.Color ?? Color.Orange,
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        .WithName("Unlocked Roles")
                        .WithValue(string.Join(", ", rolesToAdd.Select(a=> $"<@&{a}>")))
                        .WithIsInline(true)
                    },
                    ThumbnailUrl = latestRole.GetIconUrl() ?? ""
                }.Build());
        }

        public const int _nextScoreTime = 25;
        public const int _updateUserTime = 240;
        public static int[] _randomScore = { 15, 60 };
        public static int[] _randomScoreServerBooster = { 20, 80 };
        public static int getMax(int lvl)
        {
            return ((lvl + 250) * (int)Math.Pow(lvl, 2.1));
        }
        public async Task AddMessageXpAsync(IUserMessage message)
        {
            if (message.Channel is null || message.Channel is not IGuildChannel ||
               !_score_channels.Contains(message.Channel.Id))
                return;

            var user = await _userService.GetUserAsync(message.Author.Id);

            if ((DateTime.UtcNow - user.last_activiy_score_time).TotalSeconds > _nextScoreTime)
            {
                var isServerBooster = (message.Author as IGuildUser) is not { PremiumSince: null };
                int[] score = isServerBooster ? _randomScoreServerBooster : _randomScore;

                user.activiy_score += Global.Main.Random
                    .Next(score[0], score[1]);

                user.last_activiy_score_time = DateTime.UtcNow;

                await LevelUpUserAsync(user, message);
            }

            if ((DateTime.UtcNow - user.last_update).TotalSeconds > _updateUserTime)
                await _userService.UpdateAsync(user);

        }
        public async Task<bool> LevelUpUserAsync(User _user, IUserMessage message)
        {
            var max = getMax(_user.activiy_level);
            if (_user.activiy_score > max)
            {
                _user.activiy_score = _user.activiy_score - max;
                _user.activiy_level++;

                await _userService.UpdateAsync(_user);

                var result = await SyncMemberRolesAsync(message.Author as IGuildUser, _user, message.Channel as ITextChannel)
                    .TryAsync();
                return result;
            }
            return false;
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

    }

}
