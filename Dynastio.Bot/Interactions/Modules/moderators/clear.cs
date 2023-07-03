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
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    //[DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public class clearModule : CustomInteractionModuleBase
    {
        [SlashCommand("clear", "clear messages")]
        public async Task clear(int count, Direction direction = Direction.Before, string fromMessageId = "")
        {
            await DeferAsync();

            var channel = Context.Channel as ITextChannel;

            IEnumerable<IMessage> messages;

            if (string.IsNullOrEmpty(fromMessageId))
            {
                messages = await channel.GetMessagesAsync(count).FlattenAsync();
            }
            else
            {
                if (ulong.TryParse(fromMessageId, out ulong _fromMessageId))
                    messages = await channel.GetMessagesAsync(_fromMessageId, direction, count).FlattenAsync();
                else
                {
                    await FollowupAsync($"wrong message Id !");
                    return;
                }
            }

            messages = messages.Where(x => (DateTime.UtcNow - x.CreatedAt.UtcDateTime).TotalDays < 14)
                .ToList();

            await channel.DeleteMessagesAsync(messages);

            await FollowupAsync($"done, {count} messages deleted.");
        }
    }

}
