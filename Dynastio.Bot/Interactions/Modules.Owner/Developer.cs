
using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Global.Helper;
using Dynastio.Net;
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
            var target = (IMessageChannel)channel ?? await user?.CreateDMChannelAsync() ?? Context.Channel as IMessageChannel;

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
                        "❌ Error",
                        Color.Red,
                        EmoteService.GetEmoteByName("robot").Url
                    ),
                    AlertType.Warning => (
                        "⚠️ Warning",
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

                await target.SendMessageAsync(user.Mention, embed: embed);
            }
            else
            {
                await target.SendMessageAsync($"{user.Mention} {message}");
            }

            await FollowupAsync("✉️ Your message has been sent!", ephemeral: true);
        }




        public DynastioItemsService ItemsService { get; set; }
        public AiChatService Ai { get; set; }


        [MessageCommand("Answer")]
        public async Task AnswerAsync(IMessage message)
        {
            await RespondAsync("I am thinking ..", ephemeral: true);

            SocketUserMessage msg = message as SocketUserMessage;
            var buser = await this.Context.UsersService.GetOrCreateUserAsync(msg.Author.Id);

            await Ai.ReplyMessageAsync(msg, buser);
        }

        //        [SlashCommand("items", "items config")]
        //        public async Task items(string item)
        //        {
        //            await DeferAsync();
        //            // 1) Lookup
        //            if (!ItemsService.TryGetItem(item.ToLowerInvariant().Trim(), out var target))
        //            {
        //                await FollowupAsync($"❌ Cannot find item `{item}`.", ephemeral: true);
        //                return;
        //            }


        //            string systemPrompt = @"
        //Important:
        //- You are Dynast.io Bot, the official AI assistant on our Dynast.io Discord server.
        //- Always base your answers on the Dynast.io game data provided.
        //- Use clear, non-technical language so all members can understand.
        //- Apply Discord Markdown for formatting and use @Mentions instead of raw IDs.

        //Analyze this Dynast.io game item and explain it clearly:
        //";

        //            // 3) Query
        //            string aiResponse = await Ai.QueryAsync(null, systemPrompt + Newtonsoft.Json.JsonConvert.SerializeObject(target));

        //            // 4) Send back
        //            await FollowupAsync(aiResponse);
        //        }

        //[SlashCommand("RunTime", "runtime")]
        //public async Task RunTime()
        //{
        //    await Loading(10);
        //    await DeferAsync(true);

        //    await RespondAsync($"done.", ephemeral: true);
        //}



    }
}
