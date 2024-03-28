using Discord;
using Discord.Interactions;
using Dynastio.Bot.Addons;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.buttons;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{
    
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    public class AdminModule : BotInteractionModuleBase
    {
        public DynastioApi dynastio { get; set; }
        public DynastioGraphic dynastioGraphic { get; set; }


        [SlashCommand("admin", "admin menu")]
        [RateLimit(6, 1)]
        public async Task admin()
        {
            await DeferAsync();


            var subscription = BotGuild.Subscription;
            var subscriptionModule = new EmbedBuilder()
            {
                Title = $"Subscription {(BotGuild.HasSubscription() ? "Activated" : "Not Activated")}",
                Description = $"" +
                 $"Subscribed from {subscription.StartedAt.UnixTimestampDiscordFormat()} until {subscription.EndsAt.UnixTimestampDiscordFormat()}\n" +
                 $"Subscribed By: <@{subscription.UserId}> \n",
                Color = BotGuild.HasSubscription() ? Color.Green : Color.Red,
                ThumbnailUrl = BotAvatarUrl,
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = "Partners",
                        Value = $"Partner Role: {BotGuild.GetRole(Database.GuildRoleType.SubscriptionGuildAdmin).ToDiscordRole()}\n"
                    }
                }
            }.Build();

            var rank = BotGuild.RankingSettings;

            var rankingModule = new EmbedBuilder()
            {
                Title = $"Ranking Module Is {(rank.IsEnabled ? "Enabled" : "Disabled")}",
                Description = "Ranking users based on the number of messages they send.",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = "Message Xp",
                        Value =
                                $"Message Xp: ` {rank.XpPerMessage} `\n" +
                                $"Boosters Xp: ` {rank.XpBoosters} `\n" +
                                $"Random Xp: ` {rank.XpRandom} `\n" +
                                $"Delay: ` {rank.Delay} `\n",
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = "Reward",
                        Value =
                                  $"Game Coins: {(rank.IsGameRewardEnabled ? "**Supported**" : "**Not Supported**")}\n" +
                                  $"Roles Prefix: ` {rank.RolesPrefix} `\n"
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = "Xp Channels",
                        Value =
                                 $"Level Up Channel: <#{rank.LogChannelId}>\n" +
                                 $"Xp channels: {string.Join(", ", rank.ChannelIds?.Select(a => $"<#{a}>") ?? null)}\n"
                    }
                },
                Color = rank.IsEnabled ? Color.Green : Color.Red,
            }.Build();

            
            await FollowupAsync(
                text: Context.User.Mention,
                embeds: new Embed[] { rankingModule, subscriptionModule },
                components: GetComponent());

        }


        private MessageComponent GetComponent()
        {
            var cBuilder = new ComponentBuilder()
                // .WithButton(PlayersButton.GetButton(userLocale, dynastio.OnlinePlayers.Count), 0)
                ;

            cBuilder.WithButton(CancelButton.GetButton(userLocale), 3);

            return cBuilder.Build();
        }

    }
}
