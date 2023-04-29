using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions.Modules.Guild
{
    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RateLimit(60, 5, RateLimit.RateLimitType.User)]
    public class Changelog : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public DynastioClient Dynastio { get; set; }

        [SlashCommand("changelog", "dynast.io Changelog")]
        public async Task changelog(string search = "txt", int page = 1, DynastioProviderType provider = DynastioProviderType.Main)
        {
            await DeferAsync();
            var changelog = Dynastio[provider].ChangeLog;
            try
            {
                var pages = changelog.Split(new[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (search != "txt") pages = pages.Where(a => a.ToLower().Contains(search.ToLower())).ToList();

                var componenets = new ComponentBuilder();
                componenets.WithButton(this["previous"], $"changelog.buttons:{search}:{page - 1}", ButtonStyle.Primary, new Emoji("⏪"), null, page < 2, 0);
                componenets.WithButton(this["next"], $"changelog.buttons:{search}:{page + 1}", ButtonStyle.Primary, new Emoji("⏩"), null, pages.Count <= page, 0);

                var content = pages.Skip(page - 1).FirstOrDefault() ?? this["data.not_found.description"];
                var contentLiens = content.Split("\n");
                var version = string.IsNullOrWhiteSpace(contentLiens[0]) ? contentLiens[1] : contentLiens[0];

                if (search != "txt")
                {
                    content = content.Replace(search, $"**{search}**");
                    content += $"\n\n" + this["search_for:*", search];
                }
                content += $"\n\n" +
                           this["page:*", $"{page}/{pages.Count}\n"] +
                           this["closes:*", 20.ToDiscordUnixTimeFromat()];
                var message = await FollowupAsync(Context.User.Id.ToUserMention(), components: componenets.Build(), embed: content.Remove(version).ToEmbed(version, Context.Client.CurrentUser.GetAvatarUrl()));

                await message.WhenNoResponse(Context, TimeSpan.FromSeconds(50), async (x) => { await ToNoComponent(x); });
            }
            catch
            {
                await FollowupAsync(Context.User.Id.ToUserMention(), embed: changelog.Substring(0, 800).ToEmbed("Changelog", Context.Client.CurrentUser.GetAvatarUrl()));
            }
        }
        [RequireUser]
        [RateLimit(30, 4, RateLimit.RateLimitType.User)]
        [ComponentInteraction("changelog.buttons:*:*")]
        public async Task ToplistButtons(string search, int page)
        {
            changelog(search, page).RunInBackground();
            await ToNoComponent((Context.Interaction as dynamic).Message);
        }
    }
}
