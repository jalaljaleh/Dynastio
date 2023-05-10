using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Dynastio.Net;
using Discord.WebSocket;
using Dynastio.Bot.Data;
using Dynastio.Bot.Services;

namespace Dynastio.Bot.Interactions.Modules.Owner
{

    [RequireBotOwner]
    [EnabledInDm(true)]
    [Group("owner", "config bot")]
    public class OwnerModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public IDynastioBotDatabase _database { get; set; }

        [Group("redeem-code", "dynast.io redeem codes.")]
        public class RedeemCodeModule : OwnerModule
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

        [Group("setup", "config guilds")]
        public class SetupModule : OwnerModule
        {
            [SlashCommand("display", "display official guild")]
            public async Task display()
            {
                await DeferAsync();

                var guild = await _guildService.GetOfficialGuildAsync();

                await FollowupAsync(
                    guild is null
                    ? "No any guild found!"
                    : $"The official guild.id is {guild.Id}");
            }

            [RequireContext(ContextType.Guild)]
            [RequireConfirmation("Are you sure ?","Do you want to mark this server as the official server ?")]
            [SlashCommand("set", "mark this server as official guild")]
            public async Task set()
            {
                await DeferAsync();
                await _guildService.SetOfficialGuildAsync(Context.Guild.Id);
                await FollowupAsync(true ? "done, the official server is this guild now !" : "There is a problem.");
            }

        }
    }
}
