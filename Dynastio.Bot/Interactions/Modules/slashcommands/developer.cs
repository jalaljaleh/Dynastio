using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Bot.Global;
using Dynastio.Bot.Extenstions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Services;
using Dynastio.Bot.Database;
using MongoDB.Bson;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Bot.Addons;

namespace Dynastio.Bot.Interactions.Modules.slashcommands
{
    [Group("developer", "developer")]
    [EnabledInDm(true)]
    [RequireTeamMemberAttribute]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class DeveloperModule : BotInteractionModuleBase
    {
        [Group("advertisement", "advertisement")]
        public class AdvertisementModule : BotInteractionModuleBase
        {
            public AdvertisingService advertisingService { get; set; }

            [SlashCommand("delete", "delete ads")]
            public async Task delete(string Id)
            {
                await DeferAsync(true);

                var target = advertisingService.GetRemainingAdvertises().FirstOrDefault(a => a.Id.ToString() == Id);

                var ads = advertisingService.DeleteAdvertise(target);

                await FollowupAsync(embed: "Done, the record deleted from the database.".ToEmbed("Successful Operator"));
            }
            [SlashCommand("current", "current ads")]
            public async Task current(IUser user = null)
            {
                await DeferAsync(true);

                var ads = advertisingService.GetRemainingAdvertises();

                if (user is not null)
                    ads = ads.Where(a => a.User == user.Id).ToList();

                var content = ads.ToStringTable(new string[] { "Id", "Count", "Type", "StartedAt", "UserId" },

                    a => a.Id.ToString(),
                    a => a.DisplayCount + "/" + a.Count,
                    a => (int)a.Type,
                    a => a.StartedAt.ToString("d"),
                    a => a.User);

                await FollowupAsync(
                    text: Context.User.Mention + "\n" + content.ToMarkdown());
            }

            [SlashCommand("insert", "insert new ads")]
            public async Task insert(string label, string url, AdsType type, int count, IUser user, string btnEmoji = null)
            {
                await DeferAsync(true);

                if (btnEmoji is not null)
                {
                    try
                    {
                        var emote = new Emoji(btnEmoji);
                    }
                    catch
                    {
                        await FollowupAsync("emoji not valid");
                        return;
                    }
                }

                var ad = new Advertise()
                {
                    Id = ObjectId.GenerateNewId(),
                    Label = label,
                    Url = url,
                    Count = count,
                    Type = type,
                    DisplayCount = 0,
                    StartedAt = DateTime.UtcNow,
                    User = user.Id,
                    FinishedAt = DateTime.MinValue,
                    Emoji = btnEmoji
                };
                await advertisingService.InsertAndCache(ad);

                await FollowupAsync(
                    text: Context.User.Mention,
                    embed: new EmbedBuilder()
                    {
                        Title = "Data inserted",
                        Description =
                        (
                        $"Id: {ad.Id}\n" +
                        $"Label:  {label} \n" +
                        $"Url:  {url} \n" +
                        $"type:  {type} \n" +
                        $"count:  {count} \n" +
                        $"customer:  <@{user.Id}> \n" +
                        $"").ToMarkdown(),
                        ThumbnailUrl =
                        Context.Client.CurrentUser.GetAvatarUrl() ??
                        Context.Client.CurrentUser.GetDefaultAvatarUrl() ??
                        Context.Guild.IconUrl,
                    }.Build());
            }
        }
    }

}
