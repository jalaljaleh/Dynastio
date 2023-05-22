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
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("owner", "config bot")]
    public class OwnerModule : CustomInteractionModuleBase
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public RankService _rankService { get; set; }
        public IDynastioBotDatabase _database { get; set; }


        [Group("setup", "setup")]
        public class setupModule : OwnerModule
        {
            [SlashCommand("ticket", "ticket.")]
            public async Task ticket(string content, string title, string description, string imageUrl, ITextChannel channel)
            {
                await DeferAsync(true);

                await channel.SendMessageAsync(
                    content,
                    embed: description.ToEmbed(title, imageUrl, color: Color.Orange)
                    , components: new ComponentBuilder()
                    .WithButton("btn.public.ticket.start", "Start", ButtonStyle.Success, Emoji.Parse("📩"))
                    .Build());
                await FollowupAsync("done");
            }

        }


        [Group("bot", "bot")]
        public class botModule : OwnerModule
        {
            [SlashCommand("shutdown", "shutdown.")]
            public async Task shutdown()
            {
                await DeferAsync(true);
                await FollowupAsync($"done");
                Environment.Exit(0);
            }

        }

        [Group("xp", "xp")]
        public class XPModule : OwnerModule
        {
            [SlashCommand("add", "add xp to user.")]
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
            public async Task send(IUser user, string reason, RedeemCode.RedeemType type)
            {
                await DeferAsync(true);

                var code = await _database.GetRedeemCodeAsync(type);
                if (code is null)
                {
                    await FollowupAsync($"not found", ephemeral: true);
                    return;
                }

                var result = await user.SendMessageAsync(
                    $"You just got a redeem code from {userMention} for ` {reason} `\n" +
                    $"```{code.Code}```")
                    .TryAsync();

                if (result.isSuccesful)
                    await _database.DeleteAsync(code);
                else await FollowupAsync("user dm is closed.");
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
