using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Services;
using Dynastio.Bot.Data;

namespace Dynastio.Bot.Interactions.Modules.Developer
{

    [EnabledInDm(true)]
    [RequireContext(ContextType.Guild)]
    [RateLimit(60, 2, RateLimit.RateLimitType.User)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotGuildRole(Data.Guild.BotGuildRoleType.Developer)]
    [RequireGuildOfficial]
    [Group("developer", "dynast.io developer commands.")]
    public class DeveloperModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public InternetService _internetService { get; set; }
        public IDynastioBotDatabase _database { get; set; }

        [Group("redeem-code", "dynast.io redeem codes.")]
        public partial class RedeemCodeModule : DeveloperModule
        {
            [SlashCommand("add", "add redeem codes. consider separate with , or newline.")]
            public async Task add(RedeemCode.RedeemType type, IAttachment txtFile)
            {
                await DeferAsync();

                var txt = await _internetService.GetAsync(txtFile.Url);
                string[] codes = txt.Contains(",")
                    ? txt.Split(new string[] { "," }, StringSplitOptions.TrimEntries)
                    : txt.Split(new string[] { "\n", "\r", "\n\r", "\r\n" }, StringSplitOptions.TrimEntries);

                List<RedeemCode> redeemCodes = new();
                foreach (var code in codes)
                {
                    redeemCodes.Add(new RedeemCode()
                    {
                        Code = code,
                        Type = type
                    });
                }
                await _database.InsertManyAsync(redeemCodes);

                await FollowupAsync($"done, {redeemCodes.Count} redeem codes added to the db as {type}.");
            }



            [SlashCommand("status", "get status about the redeem codes.")]
            public async Task status()
            {
                await DeferAsync();

                var codes = await _database.GetRedeemCodesAsync();

                var clist = codes.GroupBy(a => a.Type).ToList();

                string table = "#  Type          Count";
                foreach (var c in clist)
                {
                    table += "\n  " + c.First().Type.ToString() + c.Count().ToString().PadLeft(10);
                }

                await FollowupAsync(embed: table.ToMarkdown().ToEmbed());
            }
        }
    }
}
