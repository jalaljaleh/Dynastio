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
    [Group("role", "role manager")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class RoleModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ViewChannel | ChannelPermission.ReadMessageHistory)]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RateLimit(60, 10, RateLimit.RateLimitType.User)]
        [Group("assignment", "Role Assignment")]
        public class AssignmentModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
        {
            [SlashCommand("add", "add a button to the assignment post")]
            public async Task add(IRole role, string title, ButtonStyle style, string emoji, string MessageId)
            {
                await DeferAsync(ephemeral: true);

                var channel = (ITextChannel)Context.Channel;

                bool isValidEmoji = Emoji.TryParse(emoji, out Emoji emoji_);
                if (!string.IsNullOrWhiteSpace(emoji) && !isValidEmoji)
                {
                    await FollowupAsync("Emoji is not valid", ephemeral: true);
                    return;
                }

                var message = await channel.GetMessageAsync(ulong.Parse(MessageId));
                if (message == null)
                {
                    await FollowupAsync($"Message not found, make sure the bot have access to the channel and the history !", ephemeral: true);
                    return;
                }
                if (message.Author.Id != Context.Client.CurrentUser.Id)
                {
                    await FollowupAsync($"Can not edit this message.", ephemeral: true);
                    return;
                }
                var embed = message.Embeds.FirstOrDefault();
                if (embed != null && embed.Author.HasValue && !embed.Author.Value.Url.Contains(Context.User.Id.ToString()))
                {
                    await FollowupAsync($"You can't modify this messsage.", ephemeral: true);
                    return;
                }

                var components = ComponentBuilder.FromMessage(message);

                if (components.ActionRows != null)
                {
                    if (components.ActionRows.Select(a => a.Components.Count).ToList().Sum() >= 20)
                    {
                        await FollowupAsync("You can't add more than 20 buttons !", ephemeral: true);
                        return;
                    }

                    if (components.ActionRows.Where(a => a.Components.Where(a => a.CustomId == $"role:{role.Id}").Any()).Any())
                    {
                        await FollowupAsync("You can't add two button with same role !", ephemeral: true);
                        return;
                    }
                }
                components.WithButton(title, $"role:{role.Id}", style, emoji_ ?? null);
                await (message as IUserMessage).ModifyAsync(x =>
                {
                    x.Components = components.Build();
                });

                await FollowupAsync("The button added to the post, you can press the button to get\\lost the role.", ephemeral: true);
            }

            [SlashCommand("remove", "remove a role button from the assignment post")]
            public async Task remove(string MessageId, [Autocomplete()] string button)
            {
                await DeferAsync(ephemeral: true);

                var channel = Context.Channel;
                var message = await channel.GetMessageAsync(ulong.Parse(MessageId));

                if (message.Author.Id != Context.Client.CurrentUser.Id)
                {
                    await FollowupAsync($"Can not edit this message.", ephemeral: true);
                    return;
                }

                var embed = message.Embeds.FirstOrDefault();
                if (embed != null && embed.Author.HasValue && !embed.Author.Value.Url.Contains(Context.User.Id.ToString()))
                {
                    await FollowupAsync($"You can't modify this messsage.", ephemeral: true);
                    return;
                }

                var components = ComponentBuilder.FromMessage(message);

                components.ActionRows.ForEach(a => a.Components.Remove(a.Components.OfType<ButtonComponent>().First(a => a.CustomId == button)));

                if (components.ActionRows.Select(a => a.Components.Count).ToList().Sum() < 1)
                    components = new ComponentBuilder();

                await (message as IUserMessage).ModifyAsync(x =>
                {
                    x.Components = components.Build();
                });

                await FollowupAsync("The button removed from the post successfuly", ephemeral: true);
            }

            [AutocompleteCommand("button", "remove")]
            public async Task autocomplete_remove()
            {
                var data = (Context.Interaction as SocketAutocompleteInteraction).Data.Options.First(a => a.Name == "message-id").Value.ToString();

                var channel = Context.Channel;

                var message = await channel.GetMessageAsync(ulong.Parse(data));
                if (message == null) return;

                var components = ComponentBuilder.FromMessage(message);
                if (components == null || components.ActionRows == null || components.ActionRows.Count == 0) return;

                List<AutocompleteResult> results = new();
                components.ActionRows.ForEach(x => x.Components.ForEach(b =>
                {
                    results.Add(new AutocompleteResult()
                    {
                        Name = (b as ButtonComponent).Label,
                        Value = (b as ButtonComponent).CustomId
                    });
                }));
                await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(results);
            }

        }
        [UnmodifiableContent]
        [ComponentInteraction($"role:*", true)]
        [RateLimit(15, 4, RateLimit.RateLimitType.User)]
        [RateLimit(5, 15, RateLimit.RateLimitType.Guild)]
        [RateLimit(10000, 3, RateLimit.RateLimitType.User, RateLimit.RateLimitBaseType.BaseOnMessageComponentCustomId)]
        public async Task Role(string roleId)
        {
            await DeferAsync(ephemeral: true);

            var role = Context.Guild.GetRole(ulong.Parse(roleId));

            var user = Context.User as IGuildUser;

            bool hasRole = user.RoleIds.Contains(role.Id);

            if (hasRole)
                await (Context.User as IGuildUser).RemoveRoleAsync(role);
            else await (Context.User as IGuildUser).AddRoleAsync(role);

            await FollowupAsync(ephemeral: true, embed: $"You {(hasRole ? "are not" : "are")} a member of {MentionUtils.MentionRole(role.Id)}, you can change it again.".ToWarnEmbed(this["successful_operation.title"])); ;
        }
    }
}
