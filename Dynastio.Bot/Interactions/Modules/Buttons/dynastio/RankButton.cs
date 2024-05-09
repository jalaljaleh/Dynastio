using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Buttons.dynastio
{
    public class RankButton : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }
        public RankingService rankingService { get; set; }

        public const string CustomId = "btn.bot.rank";
        public static Emoji Emoji => new Emoji("💫");
        public static ButtonBuilder GetButton(Locale locale)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.bot.rank.label"],
                Style = ButtonStyle.Primary,
                Emote = Emoji,
                IsDisabled = false,
                Url = null,
                CustomId = CustomId
            };
        }
        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var guild = Context.BotGuild;

            //  guild.RankingSettings.IsEnabled = true;


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

            var guildRankRoles = rankingService.GetRankingRoles(Context.Guild, guild.RankingSettings.RolesPrefix);
            if (guildRankRoles is null || guildRankRoles.Count() == 0)
            {
                await ModifyCurrentMessageAsync(embed: userLocale["embed.rank.error.guild_ranking_disabled.description"].ToInformEmbed(userLocale["embed.rank.error.guild_ranking_disabled.title"], BotAvatarUrl))
                    .TryAsync();

                await rankingService.SetUnqualifiedGuildAsync(Context.BotGuild,Context.Guild);
                return;
            }

            var userRankRoles = rankingService.GetUserRankingRoles(Context.User as IGuildUser, guildRankRoles);
            if (userRankRoles is null || userRankRoles.Count() == 0)
            {
                await rankingService.SynchronizeUserRolesAsync(Context.BotGuild, Context.User as IGuildUser, Context.BotUser.GetRankingProfile(Context.Guild.Id).Level);
                userRankRoles = rankingService.GetUserRankingRoles(Context.User as IGuildUser, guildRankRoles);
            }

            var currentRole = userRankRoles.Last();

            var embed = new EmbedBuilder()
            {
                Title = userLocale["embed.rank.title", rank.Level],
                Description =
                userLocale["embed.rank.description", currentRole.Mention, rank.Level, rank.Xp, rank.Level + 1, RankingService.GetLevelUpRequirementXp(rank)] +
                "\n\n" +
                advertisingService.GetInlineEmbedDescription(),

                ThumbnailUrl = currentRole.GetIconUrl() ?? currentRole.Guild.IconUrl ?? Context.User.TryGetAvatarUrl(),
                Color = currentRole.Color
            }.Build();


            await ModifyCurrentMessageAsync(embed: embed);
        }

    }
}
