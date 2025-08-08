using Amazon.Runtime;
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Services;
using Dynastio.Graphic;
using Dynastio.Net;
using System.ComponentModel;
using System.Linq;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{

    [RequireContext(ContextType.Guild)]
    public class MeModule : BotInteractionModuleBase
    {
        [SlashCommand("me", "me")]
        public async Task me()
        {

            var profile = Context.BotUser.GetServerProfile(Context.Guild.Id);

            var embed = new EmbedBuilder()
            {
                Title = $"Level {profile.Level}",
                Description =
                $"### Your ranking **Level** is {profile.Level} and your **Xp** is {profile.Xp}. \n\n" +
                $"## Account Details:\n" +
                $"` Youtube Channel `: `. {Context.BotUser.youtube_channel} .`\n" +
                $"` Connected Accounts `: {Context.BotUser.Accounts.Count}\n" +
                $"` Account Connection `: {Context.BotUser.IsAccountConnected}\n" +
                $"",
                ThumbnailUrl = this.BotAvatarUrl,
            }.Build();

            await RespondAsync(this.Context.User.Mention, embed: embed);
        }
    }
}
