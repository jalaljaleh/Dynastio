using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Net;
using MongoDB.Bson;
   

namespace Dynastio.Bot.Interactions.modules.moderators
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    //[DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public class clearModule : CustomInteractionModuleBase
    {
        [SlashCommand("clear", "clear messages")]
        public async Task clear(int count, IGuildUser user = null, Direction direction = Direction.Before, string fromMessageId = "")
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
            Func<IMessage, bool> filterUser = new Func<IMessage, bool>(x => user != null ? x.Author.Id == user.Id : true);

            messages = messages.Where(x => (DateTime.UtcNow - x.CreatedAt.UtcDateTime).TotalDays < 14 && filterUser.Invoke(x))
                .ToList();

            await channel.DeleteMessagesAsync(messages);

            await FollowupAsync($"done, {count} messages deleted.");
        }
    }

}
