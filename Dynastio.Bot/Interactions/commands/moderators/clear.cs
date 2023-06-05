using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;

namespace Dynastio.Bot.Interactions.Modules
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    //[DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public class clearModule : CustomInteractionModuleBase
    {
        [SlashCommand("clear", "clear messages")]
        public async Task clear(int count, Direction direction = Direction.Before, ulong fromMessageId = 0)
        {
            await DeferAsync();

            var channel = Context.Channel as ITextChannel;

            IEnumerable<IMessage> messages;

            if (fromMessageId != 0)
                messages = await channel.GetMessagesAsync(fromMessageId, direction, count).FlattenAsync();
            else
                messages = await channel.GetMessagesAsync(count).FlattenAsync();

            await channel.DeleteMessagesAsync(messages);

            await FollowupAsync($"done, {count} messages deleted.");
        }
    }

}
