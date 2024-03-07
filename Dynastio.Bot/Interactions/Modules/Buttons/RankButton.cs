using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.buttons
{
    public class RankButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }

        public const string CustomId = "btn.bot.rank";
        public static Emoji Emoji => new Emoji("💫");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.bot.rank.label"],
                Style = ButtonStyle.Primary,
                Emote = RankButton.Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = RankButton.CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var guild = Context.BotGuild;

            if (guild.RankingSettings.IsEnabled is false)
            {
                await ModifyCurrentMessageAsync(embed: userLocale["embed.rank.error.guild_ranking_disabled.description"].ToInformEmbed(userLocale["embed.rank.error.guild_ranking_disabled.title"], BotAvatarUrl));
                return;
            }

            var rank = BotUser.GetRankingProfile(guild.Id);
            if (rank.Level == 0)
            {
                await ModifyCurrentMessageAsync(embed: userLocale["embed.rank.not_ranked.description"].ToInformEmbed(userLocale["embed.rank.not_ranked.title"], BotAvatarUrl));
                return;
            }

            var prefix = guild.RankingSettings.RolesPrefix;

            var roles = Context.Guild.Roles
                              .Where(x => x.Name.StartsWith(prefix + " "))
                              .OrderBy(a => a.Position)
                              .ToList();

            // Rules are not created or not match with the prefix
            if (roles is null || roles.Count == 0)
            {
                guild.RankingSettings.IsEnabled = false;
                await dynastioBotDatabase.UpdateAsync(guild);
                await ModifyCurrentMessageAsync(embed: userLocale["embed.rank.error.guild_ranking_disabled.description"].ToInformEmbed(userLocale["embed.rank.error.guild_ranking_disabled.title"], BotAvatarUrl));
                return;
            }

            var roleIds = roles.Select(a => a.Id);

            SocketRole currentRole = roles.Count > rank.Level
                ? roles[rank.Level - 1]
                : roles.Last();

            SocketRole nextRole = roles.Count > rank.Level
                ? roles[rank.Level]
                : roles.Last();


            var embed = new EmbedBuilder()
            {
                Title = userLocale["embed.rank.title", rank.Level],
                Description =
                userLocale["embed.rank.description", currentRole.Mention, rank.Level, rank.Xp, nextRole.Mention, rank.Level + 1, RankingService.GetLevelUpRequirementXp(rank)] +
                "\n\n" +
                advertisingService.GetInlineEmbedDescription(),

                ThumbnailUrl = currentRole.GetIconUrl() ?? currentRole.Guild.IconUrl ?? Context.User.TryGetAvatarUrl(),
                Color = currentRole.Color
            }.Build();


            await ModifyCurrentMessageAsync(embed: embed);
        }

    }
}
