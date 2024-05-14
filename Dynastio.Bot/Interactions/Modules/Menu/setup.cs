using Discord.Interactions;
using Discord;
using Dynastio.Bot.Globalization;
using Dynastio.Bot.Interactions.Modules.Modals;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Interactions.Modules.buttons;
using Discord.WebSocket;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions.Modules.Buttons.dynastio;
using Dynastio.Bot.Interactions.Modules.shared_buttons;
using Dynastio.Bot.Services;
using Amazon.Runtime.Internal.Util;

namespace Dynastio.Bot.Interactions.Modules.Menu
{
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    public class SetupModule : BotInteractionModuleBase, IMenuModule
    {
        public const string CustomId = "btn.menu.setup";
        public static Emoji Emoji => new Emoji("⚙️");
        public static ButtonBuilder GetButton(Locale locale, bool IsDisabled = false)
        {
            return new ButtonBuilder()
            {
                Label = locale["btn.menu.setup.label"],
                Style = ButtonStyle.Success,
                Emote = Emoji,
                IsDisabled = IsDisabled,
                Url = null,
                CustomId = CustomId
            };
        }

        [SlashCommand("setup", "setup menu")]
        public async Task SlashCommandAsync()
        {
            await ExecuteAsync();
        }

        [RequireComponentMessageMention]
        [ComponentInteraction(CustomId)]
        public async Task ButtonAsync()
        {
            await ExecuteAsync();
        }

        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var componentResult = TryCreateComponents(out ComponentBuilder component);
            var embedsResult = TryCreateEmbeds(out Embed[] embeds);

            if (Context.Interaction is SocketMessageComponent)
                await ModifyCurrentMessageAsync(userMention, embeds: embeds, components: component.Build());
            else
                await FollowupAsync(
                    text: TryCreateTextContent(),
                    embeds: embeds,
                    components: component.Build());
        }

        public string TryCreateTextContent()
        {
            //var contentAdvertises = advertisingService.ExploitationAdvertising(Database.AdsType.MessageContent, 1).FirstOrDefault();
            //return Context.User.Mention + " | " + contentAdvertises?.GetEmbedLink();
            return userMention;
        }
        public bool TryCreateEmbeds(out Embed[] embeds)
        {
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
                        Value = $"Partner Role: {BotGuild.PartnersRoleId.ToDiscordRole()}\n"
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

            var badgeRoles = BotGuild.BadgesRole;
            var badgeRolesContent = badgeRoles.Roles.Any()
                ? $"### badge roles:\n" + string.Join("\n", badgeRoles.Roles?.Select(a => string.Format("{0}: <@&{1}>", a.Badge, a.RoleId)))
                : "";
           var badgeRolesModule = new EmbedBuilder()
            {
                Title = $"BadgeRole Module is {(badgeRoles.IsEnabled ? "Enabled" : "Disabled")}",
                Description = badgeRolesContent,
                Color = badgeRoles.IsEnabled ? Color.Green : Color.Red,
                //ThumbnailUrl = "",
                //Fields = new List<EmbedFieldBuilder>()
                //{
                //    new EmbedFieldBuilder()
                //    {
                //        IsInline = true,
                //        Name = "",
                //        Value = $"\n"
                //    }
                // }
            }.Build();

            embeds = new Embed[] { subscriptionModule, rankingModule, badgeRolesModule};
            return true;
        }
        public bool TryCreateComponents(out ComponentBuilder component)
        {
            component = new ComponentBuilder();
            component.WithButton(CancelButton.GetButton(userLocale), 0);
            component = advertisingService.ExploitationAdvertisingButtons(component, 0, 4);
            return true;
        }


    }
}
