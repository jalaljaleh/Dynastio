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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Reflection;
using Dynastio.Bot.Interactions.AutoCompeletes;

namespace Dynastio.Bot.Interactions.modules
{

    [RequireBotOwner]
    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("owner", "config bot")]
    public class OwnerModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public RankService _rankService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public DynastioClient _dynastio { get; set; }
        public IDynastioBotDatabase _database { get; set; }

        [RequireRole(480954902005415937)]
        [SlashCommand("test-nightly", "test.")]
            public async Task add(string gameId, int level)
            {
                await DeferAsync(true);

                var res = await _dynastio.UpdateDiscordRank(gameId, new DiscordRank() { Rank = level })
                    .TryAsync();
                await FollowupAsync($"{(res.isSuccesful ? res.result.Rank : "false")}");
            }

        
        [Group("xp", "xp")]
        public class XPModule : OwnerModule
        {
            [SlashCommand("add", "add xp to targrtUser.")]
            public async Task add(IUser user, int count, string reason)
            {
                await DeferAsync(true);

                var target = await _userService.GetUserAsync(user.Id, false);
                if (target is null)
                {
                    await FollowupAsync($"not found");
                    return;
                }
                await _rankService.AddXpAsync(target, Context.User.Id, count, reason);

                await FollowupAsync($"done");
            }

        }

        [Group("redeem-code", "dynast.io redeem codes.")]
        public class RedeemCodeModule : OwnerModule
        {
            [SlashCommand("send", "send .")]
            public async Task send(IUser targrtUser, string reason, RedeemCode.RedeemType type)
            {
                await DeferAsync(true);

                var code = await _database.GetRedeemCodeAsync(type);
                if (code is null)
                {
                    await FollowupAsync($"not found", ephemeral: true);
                    return;
                }

                var result = await targrtUser.SendMessageAsync(
                    $"You just got a redeem code for ` {reason} `\n" +
                    $"```{code.Code}```")
                    .TryAsync();

                if (result.isSuccesful)
                {
                    await _database.DeleteAsync(code);
                    await FollowupAsync("done.");

                    await _discord.GetGuild(GuildService._officialGuildId)
                                    .GetTextChannel(RankService._scoreChannelId)
                                    .SendMessageAsync(
                                    text: targrtUser.Id.ToUserMention(),
                                    embed: new EmbedBuilder()
                                    {
                                        Title = $"🎉 You just got {type} redeem code!",
                                        Description = $"You got **{type}** redeem code for ` {reason} `",
                                        Color = Color.Green,
                                        ThumbnailUrl =
                                        "https://cdn.discordapp.com/attachments/1111209352095871028/1111209352217509938/openiron.png",
                                    }.Build());
                }
                else await FollowupAsync("targrtUser dm is closed.");
            }

            [SlashCommand("add", "add redeem codes. consider separate with , or newline.")]
            public async Task add(IAttachment txtFile, RedeemCode.RedeemType type)
            {
                await DeferAsync();

                var txt = await _internetService.GetAsync(txtFile.Url);

                string[] codes = txt.Contains(",")
                    ? txt.Split(new string[] { "," }, StringSplitOptions.TrimEntries)
                    : txt.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

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

            [SlashCommand("list", "get status about the redeem codes.")]
            public async Task list()
            {
                await DeferAsync();

                var codes = await _database.GetRedeemCodesAsync();

                var clist = codes
                    .GroupBy(a => a.Type)
                    .ToList();

                string table = clist.ToStringTable(new string[] { "Type", "Remain" },
                    a =>
                    a.FirstOrDefault().Type,
                    a => a.Count());

                await FollowupAsync(embed: table.ToMarkdown().ToEmbed());
            }
        }


    }
}
