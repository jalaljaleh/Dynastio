using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace YourNamespace.Modules
{
    public enum MenuPage
    {
        Home,
        Players,
        Profile,
        Search
    }

    public class MenuModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("menu", "Open the main menu")]
        public async Task OpenMenu()
        {
            var ownerId = Context.User.Id;
            var embed = MenuUi.BuildEmbed(MenuPage.Home, Context.Client.CurrentUser);
            var comps = MenuUi.BuildComponents(MenuPage.Home, ownerId);

            await RespondAsync(embed: embed, components: comps, ephemeral: false);
        }

        // Handles all buttons that start with "menu:"
        [ComponentInteraction("menu:*")]
        public async Task OnMenu(string payload)
        {
            // custom_id shape: menu:{page}:{ownerId}
            // Example: menu:profile:123456789012345678
            var parts = payload.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                await RespondAsync("Invalid menu action.", ephemeral: true);
                return;
            }

            var pageStr = parts[0];
            ulong ownerId = 0;
            _ = parts.Length >= 2 ? ulong.TryParse(parts[^1], out ownerId) : false;

            // Access control: only the user who opened the menu can use it
            if (ownerId != 0 && Context.User.Id != ownerId)
            {
                await RespondAsync("This menu isn’t yours.", ephemeral: true);
                return;
            }

            // Map route to a page
            var page = pageStr.ToLowerInvariant() switch
            {
                "home" => MenuPage.Home,
                "players" => MenuPage.Players,
                "profile" => MenuPage.Profile,
                "search" => MenuPage.Search,
                "back" => MenuPage.Home,
                _ => MenuPage.Home
            };

            var embed = MenuUi.BuildEmbed(page, Context.Client.CurrentUser);
            var comps = MenuUi.BuildComponents(page, ownerId == 0 ? Context.User.Id : ownerId);

            // Update the existing message in-place
            await Context.Interaction.UpdateAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = comps;
            });
        }
    }

    internal static class MenuUi
    {
        public static Embed BuildEmbed(MenuPage page, IUser botUser)
        {
            var botIcon = botUser.GetAvatarUrl() ?? botUser.GetDefaultAvatarUrl();

            var title = page switch
            {
                MenuPage.Home => "Main Menu",
                MenuPage.Players => "Players",
                MenuPage.Profile => "Your Profile",
                MenuPage.Search => "Search",
                _ => "Menu"
            };

            var description = page switch
            {
                MenuPage.Home => "Choose a section below.",
                MenuPage.Players => "Browse or manage players.",
                MenuPage.Profile => "View or edit your profile.",
                MenuPage.Search => "Find items, entities, or badges.",
                _ => "Choose an option."
            };

            return new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(Color.Blue)
                .WithThumbnailUrl(botIcon)
                .WithCurrentTimestamp()
                .Build();
        }

        public static MessageComponent BuildComponents(MenuPage page, ulong ownerId)
        {
            var builder = new ComponentBuilder();

            // Row 1: primary navigation
            if (page != MenuPage.Players)
                builder.WithButton("Players", Custom("players", ownerId), ButtonStyle.Primary, row: 0);
            if (page != MenuPage.Profile)
                builder.WithButton("Profile", Custom("profile", ownerId), ButtonStyle.Primary, row: 0);
            if (page != MenuPage.Search)
                builder.WithButton("Search", Custom("search", ownerId), ButtonStyle.Primary, row: 0);

            // Row 2: context actions
            switch (page)
            {
                case MenuPage.Players:
                    builder.WithButton("Refresh", Custom("players", ownerId), ButtonStyle.Secondary, row: 1);
                    break;
                case MenuPage.Profile:
                    builder.WithButton("Edit", Custom("profile-edit", ownerId), ButtonStyle.Secondary, row: 1);
                    break;
                case MenuPage.Search:
                    builder.WithButton("New Search", Custom("search", ownerId), ButtonStyle.Secondary, row: 1);
                    break;
            }

            // Row 3: navigation/back/close
            if (page != MenuPage.Home)
                builder.WithButton("Back", Custom("back", ownerId), ButtonStyle.Secondary, row: 2);

            builder.WithButton("Close", Custom("close", ownerId), ButtonStyle.Danger, row: 2);

            return builder.Build();

            static string Custom(string page, ulong id) => $"menu:{page}:{id}";
        }
    }
}
