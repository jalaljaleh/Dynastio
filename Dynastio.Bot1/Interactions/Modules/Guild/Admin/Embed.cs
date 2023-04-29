using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Dynastio.Bot.Extensions.Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Dynastio.Bot.Interactions.Modules.Guild.Admin
{
    [EnabledInDm(false)]
    [Group("embed", "embed utilities")]
    [RequireContext(ContextType.Guild)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class EmbedModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Group("builder", "Embed builder")]
        public class builderModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            [SlashCommand("create", "create an embed")]
            public async Task create()
            {
                await RespondWithModalAsync<EmbedBuilderForm>($"embed builder modal-create:0");
            }
            [RequireBotPermission(ChannelPermission.ReadMessageHistory)]
            [SlashCommand("edit", "edit an embed")]
            public async Task edit(string MessageId)
            {
                var message = await Context.Channel.GetMessageAsync(ulong.Parse(MessageId));
                var embed = message.Embeds.FirstOrDefault();
                await Context.Interaction.RespondWithModalAsync<EmbedBuilderForm>($"embed builder modal-create:{MessageId}", null,
                    x =>
                    {
                        x.UpdateTextInput("content", c => c.Value = message.Content);
                        if (embed != null)
                        {
                            x.UpdateTextInput("title", c => c.Value = embed.Title ?? "");
                            x.UpdateTextInput("description", c => c.Value = embed.Description ?? "");

                            x.UpdateTextInput("thumbnail-url", c => c.Value = embed.Thumbnail.HasValue ? embed.Thumbnail.Value.Url : "");
                            x.UpdateTextInput("image-url", c => c.Value = embed.Image.HasValue ? embed.Image.Value.Url : "");
                        }
                    });
            }
            [ModalInteraction("embed builder modal-create:*", true)]
            public async Task role_assignment_create(string edit, EmbedBuilderForm form)
            {
                await DeferAsync(true);

                var embed = new EmbedBuilder()
                {
                    Title = form.Title_,
                    Description = form.Description,
                    ThumbnailUrl = form.ThumbnailUrl,
                    ImageUrl = form.ImageUrl,
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                        Url = $"https://discord.com/channels/@me/{Context.User.Id}"
                    },

                };
                var channel = Context.Channel;

                IMessage msg = null;
                if (edit.Equals("0"))
                {
                    msg = await channel.SendMessageAsync(form.Content, embed: embed.Build());
                }
                else
                {
                    msg = await channel.GetMessageAsync(ulong.Parse(edit));
                    if (msg == null)
                    {
                        await FollowupAsync($"Message not found, make sure the bot have access to the channel and the history !", ephemeral: true);
                        return;
                    }
                    if (msg.Author.Id != Context.Client.CurrentUser.Id)
                    {
                        await FollowupAsync($"Can not edit this message.", ephemeral: true);
                        return;
                    }
                    var msgEmbed = msg.Embeds.FirstOrDefault();
                    if (msgEmbed != null && msgEmbed.Author.HasValue && !msgEmbed.Author.Value.Url.Contains(Context.User.Id.ToString()))
                    {
                        await FollowupAsync($"You can't modify this embed.", ephemeral: true);
                        return;
                    }
                    await (msg as IUserMessage).ModifyAsync(a =>
                    {
                        a.Content = form.Content;
                        a.Embed = embed.Build();
                    });
                }
                await FollowupAsync($"Done, your embed {(edit.Equals("0") ? "created" : "edited")} [Jump to the message]({msg.GetJumpUrl()})", ephemeral: true);
            }


        }

    }
}
