
using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Global.Helper;
using Dynastio.Net;
using Google.Apis.Util;
using Microsoft.VisualBasic;
using MongoDB.Bson.IO;
using System.Text.Json;

namespace Dynastio.Bot.Interactions.Modules.Owner
{
    [RequireApplicationTeamAttribute]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    [Group("developer", "developer commands")]
    public class Developer : MenuModulesBase
    {
        [SlashCommand("pincode", "bypass pin code")]
        public async Task ByPassPinCode(string newCode)
        {
            await DeferAsync(true);
            //   await (Context.User as IGuildUser).RemoveRoleAsync(1416006928898850877);
            Interactions.Modules.Menu.Buttons.ButtonLoginModule.BypassPinCode = newCode;
            await RespondAsync($"PinCode-bypass created successfuly.", ephemeral: true);
        }

        [Group("users", "users commands")]
        public class UsersModule : MenuModulesBase
        {
            [SlashCommand("data", "user data")]
            public async Task UnlinkAccount(IUser user)
            {
                await DeferAsync(true);

                var buser = await UsersService.GetOrCreateUserAsync(user.Id);

                await FollowupAsync(embed: JsonSerializer.Serialize(buser).ToEmbed("user data"), ephemeral: true);
            }
            [SlashCommand("update", "update user data")]
            public async Task Update(IUser user, string json)
            {
                await DeferAsync(true);

                var buser = await UsersService.GetOrCreateUserAsync(user.Id);
                buser = JsonSerializer.Deserialize<User>(json);

                await UsersService.UpdateUserAsync(buser);

                await FollowupAsync($"user updated", ephemeral: true);
            }
            [SlashCommand("ban", "ban a user from using the bot")]
            public async Task BanUser(IUser user, [Choice("Ban", 1), Choice("Unban", 2)] int action)
            {
                await DeferAsync(true);

                var buser = await UsersService.GetOrCreateUserAsync(user.Id);

                buser.AsBannedUser(action == 1 ? true : false);

                await UsersService.UpdateUserAsync(buser);

                await FollowupAsync($"User ban status has been updated to ` {(buser.IsBanned ? "Banned" : "Unbanned")} `.", ephemeral: true);
            }
        }
        public enum ShapeType { Items, Entities }
        [SlashCommand("shape", "description")]
        public async Task shape(int width, int height, ShapeType type)
        {
            var shapeGen = new DynastioShapeGenerator(EmoteService);

            string shape = type switch
            {
                ShapeType.Items => await shapeGen.CreateRandomShapeAsync<EntityType>(width, height),
                ShapeType.Entities => await shapeGen.CreateRandomShapeAsync<ItemType>(width, height),
                _ => null
            };


            ComponentBuilderV2 cb = new ComponentBuilderV2()
                .WithTextDisplay(shape);

            await RespondAsync(components: cb.Build());
        }

        public enum AlertType
        {
            None,
            Warning,
            Error,
            Success
        }

        [SlashCommand("message", "Send a styled embed notification to a user or channel")]
        public async Task MessageAsync(
            [Summary("message", "Message content")] string message,
            [Summary("user", "Target user")] IUser user = null,
            [Summary("channel", "Optional text channel")] ITextChannel channel = null,
            [Summary("type", "Embed style")] AlertType type = AlertType.None
        )
        {
            await DeferAsync(ephemeral: true);

            // Determine target: either specified channel or a DM
            var target = (IChannel)channel ?? await user?.CreateDMChannelAsync() ?? null;
            if (target == null)
            {
                await FollowupAsync("target not found, select a channel or a user !", ephemeral: true);
                return;
            }
            if (type != AlertType.None)
            {
                var (title, color, iconUrl) = GetEmbedStyle(type);
                (string Title, Color Color, string IconUrl) GetEmbedStyle(AlertType type) =>
                    type switch
                    {
                        AlertType.Success => (
                            "✅ You have an unread message !",
                            Color.Green,
                            EmoteService.GetEmoteByName("left_build_icon").Url
                        ),
                        AlertType.Error => (
                            "❌ Error,  You have an unread message !",
                            Color.Red,
                            EmoteService.GetEmoteByName("robot").Url
                        ),
                        AlertType.Warning => (
                            "⚠️ Warning,  You have an unread message !",
                            Color.Orange,
                            EmoteService.GetEmoteByName("premium").Url
                        ),
                        _ => (
                            string.Empty,
                            Color.Default,
                            string.Empty
                        )
                    };

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(message)
                    .WithColor(color)
                    .WithThumbnailUrl(iconUrl)
                    .WithFooter(f => f.Text = $"From {Context.User.Username}")
                    .WithTimestamp(DateTimeOffset.UtcNow)
                    .Build();

                await ((ITextChannel)target).SendMessageAsync(user.Mention, embed: embed);
            }
            else
            {
                await ((ITextChannel)target).SendMessageAsync($"{user.Mention} {message}");
            }

            await FollowupAsync("✉️ Your message has been sent !", ephemeral: true);
        }


    }
}
