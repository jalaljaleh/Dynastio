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

namespace Dynastio.Bot.Handlers.Interactions.Modules.slashcommands
{
    [Group("developer", "developer")]
    [EnabledInDm(true)]
    [RequireTeamMemberAttribute]
    public class DeveloperModule : BotInteractionModuleBase
    {
        [Group("advertisement", "advertisement")]
        public class AdvertisementModule : BotInteractionModuleBase
        {
            public AdvertisingService advertisingService { get; set; }

            [SlashCommand("insert", "insert new ads")]
            public async Task insert(string label, string url, AdsType type, int count, IUser user)
            {
                await DeferAsync();
                await advertisingService.InsertAndCache(new Database.Advertise()
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
                });

                await FollowupAsync(
                    text: Context.User.Mention,
                    embed: new EmbedBuilder()
                    {
                        Title = "Data inserted",
                        Description =
                        ($"Label:  {label} \n" +
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
