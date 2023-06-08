using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using MongoDB.Driver;

namespace Dynastio.Bot.Interactions.Modules.Guild
{

    [EnabledInDm(false)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.AttachFiles | ChannelPermission.SendMessages)]
    [Group("info", "dyanstio info commands")]
    public class InfoModule : CustomInteractionModuleBase
    {
        public DynastioClient Dynastio { get; set; }


        [RateLimit(10)]
        [SlashCommand("items", "get private server items")]
        public async Task items(bool Newest = false,int NewestNumber = 10)
        {
            await DeferAsync();

            string content = "";

            var items = (ItemType[])Enum.GetValues(typeof(ItemType));
            
            if (Newest)
                items = items.TakeLast(NewestNumber).ToArray();

            var items_ = items.GroupBy(
                    a =>
                    a.ToString()[0]
                     .ToString()
            ).OrderBy(a => a.Key)
            .ToList();

            string[] headers = items_
                .Select(a => a.Key)
                .ToArray();

            foreach (var g in items_)
                content += $"**{g.Key}:** ```" + string.Join(", ", g.Select(a => $"{a.ToString()}")) + "```";

            await FollowupAsync(embed: content.ToEmbed(Newest ? "Newest Items" : "Items List"));
        }
        [RateLimit(10)]
        [SlashCommand("entities", "get private server entities")]
        public async Task entities(bool Newest = false, int NewestNumber = 10)
        {
            await DeferAsync();

            string content = "";

            var entities = (EntityType[])Enum.GetValues(typeof(EntityType));
            if (Newest)
                entities = entities.TakeLast(NewestNumber).ToArray();

            var entitiesG = entities.GroupBy(a => a.ToString()[0].ToString()).OrderBy(a => a.Key).ToList();

            string[] headers = entitiesG.Select(a => a.Key).ToArray();
            foreach (var g in entitiesG)
                content += $"**{g.Key}:** ```" + string.Join(", ", g.Select(a => $"{a.ToString()}")) + "```";

            await FollowupAsync(embed: content.ToEmbed(Newest ? "Newest Entities" : "Entities List"));
        }
      

    }
}
