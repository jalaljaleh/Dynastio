using Discord;
using Discord.Interactions;
using Dynastio.Bot;
using Dynastio.Bot.Interactions.Modules.Menu.Menu.Interactions.Buttons;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Services;
using Dynastio.Bot.Services.GlobalizationService.Globally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Menu.Menu.Interactions
{
    [RequireContext(ContextType.Guild)]
    [RequireTeam]
    public class BuilderModule : MenuModulesBase
    {
        const string command = "menu";
        public EmoteService EmoteService { get; set; }


        [SlashCommand(command, "description")]
        public async Task menu()
        {
            await DeferAsync();

            var sb1 = new SectionBuilder()
                .WithTextDisplay($"## {EmoteService.GetEmote(Net.BadgeType.Robot)}  Dynast.io Menu !\n")
                .WithTextDisplay($"{UserMention} this is the central nexus of your [Dynast.io](https://dynast.io/) journey. This is your all‑in‑one hub to view profile, personal chest, stats, manage settings, and keep everything for your personal smoothly.\n\n**Use the buttons below to jump straight to what matters most !**")
                .WithAccessory(new ThumbnailBuilder(this.BotAvatarUrl, "Dynast.io Bot", false));

            var containerb = new ContainerBuilder()
                .WithAccentColor(Color.DarkGreen)
                .WithSection(sb1)
                .WithSeparator(SeparatorSpacingSize.Small, true)

            .WithTextDisplay("### Public")
            .WithTextDisplay("Public dynast.io commands:")
            .WithActionRow(new ActionRowBuilder()
                                .WithButton(GetPlayersButton())
                                .WithButton(GetServersButton())
                                .WithButton(GetTeamsButton())
                                .WithButton(GetSearchPlayersButton())
                 )
            .WithSeparator(SeparatorSpacingSize.Small, true);

            containerb
            .WithTextDisplay("### Personal")
            .WithTextDisplay("Personal dynast.io commands:")
            .WithActionRow(new ActionRowBuilder()
                                .WithButton(GetProfileButton())
                                .WithButton(GetRankButton())
                                .WithButton(GetSettingsButton())
                 )
             .WithActionRow(new ActionRowBuilder()
                                .WithButton(GetConnectedAccountsButton())
                                .WithButton(GetAddAccountButton())
                             //   .WithButton(GetSettingsButton())

                 )
            ;

            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithContainer(containerb);



            await FollowupAsync(components: cb.Build());
        }
        private ButtonBuilder GetPlayersButton()
        {
            var playersCount = 101;//Dynastio.OnlinePlayers.Count;
            string label = $"{playersCount} Players";
            return new ButtonBuilder()
            {
                Label = label,
                Emote = EmoteService.GetEmote(Net.EntityType.Heartstone),
                Style = ButtonStyle.Primary,
                IsDisabled = playersCount < 1,
                CustomId = ButtonPlayers.Id + $":1:20:menu"
            };
        }
        private ButtonBuilder GetServersButton()
        {
            var serversCount = 10;//Dynastio.OnlineServers.Count;
            string label = $"{serversCount} Servers";
            return new ButtonBuilder()
            {
                Label = "Servers",
                Emote = EmoteService.GetEmoteByName("map_icon"),
                Style = ButtonStyle.Primary,
                IsDisabled =serversCount < 1,
                CustomId = ButtonServers.Id + $":0:1:10:menu"
            };
        }
        private ButtonBuilder GetTeamsButton()
        {
            var teamsCount = 12;//Dynastio.OnlinePlayers.GroupBy(a=>a.Team).ToList().Count;
            string label = $"{teamsCount} Teams";
            return new ButtonBuilder()
            {
                Label = "Teams",
                Emote = EmoteService.GetEmoteByName("mainmenu_level_shield_premium"),
                Style = ButtonStyle.Primary,
                IsDisabled = teamsCount < 1,
                CustomId = ButtonPlayers.Id + $":1:00"
            };
        }
        private ButtonBuilder GetSearchPlayersButton()
        {
            return new ButtonBuilder()
            {
                Label = "Search Players",
                Emote = EmoteService.GetEmoteByName("mainmenu_level_shield_premium"),
                Style = ButtonStyle.Success,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:000"
            };
        }

        private ButtonBuilder GetProfileButton()
        {
            return new ButtonBuilder()
            {
                Label = "Profile",
                Emote = EmoteService.GetEmoteByName("tab_profile_icon_active"),
                Style = ButtonStyle.Primary,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:0000"
            };
        }
        private ButtonBuilder GetRankButton()
        {
            return new ButtonBuilder()
            {
                Label = "Rank",
                Emote = EmoteService.GetEmoteByName("tab_leaders_icon_active"),
                Style = ButtonStyle.Primary,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:00000"
            };
        }
        private ButtonBuilder GetSettingsButton()
        {
            return new ButtonBuilder()
            {
                Label = "Settings",
                Emote = EmoteService.GetEmoteByName("select_skin_button"),
                Style = ButtonStyle.Success,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:000000"
            };
        }
        private ButtonBuilder GetConnectedAccountsButton()
        {
            return new ButtonBuilder()
            {
                Label = "Connected Accounts",
                Emote = EmoteService.GetEmoteByName("left_team_icon"),
                Style = ButtonStyle.Primary,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:0000000"
            };
        }
        private ButtonBuilder GetAddAccountButton()
        {
            return new ButtonBuilder()
            {
                Label = "Add Account",
                Emote = EmoteService.GetEmoteByName("zoom_in"),
                Style = ButtonStyle.Success,
                IsDisabled = false,
                CustomId = ButtonPlayers.Id + $":1:00000000"
            };
        }
    }


}
