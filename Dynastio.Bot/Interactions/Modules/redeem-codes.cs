using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Dynastio.Data;
using Dynastio.Bot.Services;

namespace Dynastio.Bot.Interactions.modules
{
    //[RequireRole(480954902005415937)]
    //[SlashCommand("test-nightly", "test.")]
    //    public async Task add(string gameId, int level)
    //    {
    //        await DeferAsync(true);

    //        var res = await _dynastio.UpdateDiscordRank(gameId, new DiscordRank() { Rank = level })
    //            .TryAsync();
    //        await FollowupAsync($"{(res.isSuccesful ? res.result.Rank : "false")}");
    //    }

    [RequireDeveloper]
    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("redeem-code", "dynast.io redeem codes.")]
    public class RedeemCodeModule : CustomInteractionModuleBase
    {
        public InternetService _internetService { get; set; }
        public DynastioData _database { get; set; }
        public WebhookService _webhookService { get; set; }


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

                await _webhookService.LogRewardAsync(text: targrtUser.Id.ToUserMention(),
                                embeds: new Embed[]{ new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder(){Name = targrtUser.Username, IconUrl = targrtUser.GetAvailableAvatarUrl()},
                                        Title = $"🎉 You just got {type} redeem code !",
                                        Description = $"You got **{type}** redeem code for ` {reason} ` !",
                                        Color = Color.Green,
                                        ThumbnailUrl =
                                        "https://cdn.discordapp.com/attachments/1111209352095871028/1111209352217509938/openiron.png",
                                        Footer = new EmbedFooterBuilder(){Text = "Dynast.io Rewards", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() }
                                    }.Build() });
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
