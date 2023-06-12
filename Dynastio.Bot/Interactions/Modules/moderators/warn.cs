using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.modules.moderators
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [DefaultMemberPermissions(GuildPermission.ModerateMembers)]
    [Group("warn", "warn commands")]
    public class warnModule : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }

        [SlashCommand("add", "warn to a user")]
        public async Task add(IGuildUser user, [MaxLength(40)] string reason = "no reason provided")
        {
            await DeferAsync();

            var targetUser = await _userService.GetUserAsync(user.Id);

            targetUser.Warns.Add(new Data.UserWarn()
            {
                Content = reason,
                CreatedAt = DateTime.UtcNow,
                SourceId = Context.User.Id
            });

            await _userService.UpdateAsync(targetUser);

            var embed = new EmbedBuilder()
            {
                Description = $"{user.Mention} You have been warned by {userMention}.",
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Color = Color.DarkRed,
                Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason.TryRemove(40,false),
                            IsInline = true
                        },
                         new EmbedFieldBuilder()
                        {
                            Name = "Warns Count",
                            Value =  targetUser.Warns.Count,
                            IsInline = true
                        }
                    }
            }
            .Build();


            await FollowupAsync(
                text: user.Mention + " | " + userMention,
                embed: embed
                //components: new ComponentBuilder()
                //            .WithButton("Revoke Now", $"btn.mute.revoke:{user.Id}", ButtonStyle.Danger, new Emoji("🔘"))
                //            .Build()
                );
        }
        [SlashCommand("list", "warn list")]
        public async Task list(IGuildUser user,int page = 0)
        {
            await DeferAsync();

            var targetUser = await _userService.GetUserAsync(user.Id, false);

            var content = targetUser.Warns.Skip(page * 10).ToStringTable(new string[] {"#", "Created At", "Moderator", "Reason" },
                 a => targetUser.Warns.IndexOf(a) + ". ",
                 a => a.CreatedAt.ToRelative(),
                 a => a.SourceId.ToUserMention(),
                 a => a.Content);

            await FollowupAsync(
              text: userMention,
              embed: ($"{user.Mention} Warns\n" + content).ToEmbed("User Warns")
                );
        }
        [SlashCommand("remove", "remove a user warn")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task remove(IGuildUser user, int count)
        {
            await DeferAsync();

            var targetUser = await _userService.GetUserAsync(user.Id, false);
            try
            {
                targetUser.Warns.RemoveRange(0, count);

                await _userService.UpdateAsync(targetUser);
            }
            catch
            {

            }
            await FollowupAsync(
                text: userMention,
                embed: "Done, user warns remvoed.".ToEmbed()
                );
        }
    }

}
