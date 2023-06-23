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
using System.Threading;
using Discord.Rest;

namespace Dynastio.Bot.Interactions.ServiceModules
{
    public class ClanModule : CustomInteractionModuleBase
    {
        [RequireBotOwner]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("owner-setup-clans", "setup clan module.")]
        public async Task setup(ITextChannel channel, ITextChannel TargetChannel)
        {
            await DeferAsync(true);

            await channel.SendMessageAsync(
             "## ** Clans **\n" +
             "- create a clan and invite your friends.",
              components: new ComponentBuilder()
              .WithButton("Create", $"btn.clan.start:{TargetChannel.Id}", ButtonStyle.Success, Emoji.Parse("🛠"))
              .Build());

            await FollowupAsync("done");
        }

        [RateLimit(30)]
        [ComponentInteraction("btn.clan.start:*")]
        public async Task btn_ticket_start(ulong _channel)
        {
            var modal = new ModalBuilder(this["modal.clan.start.title"], $"tickets.start:{_channel}")
               .AddTextInput(new TextInputBuilder("Name", "name", TextInputStyle.Short, "its a name for your clan.", 0, maxLength: 16, null))
              .Build();

            await RespondWithModalAsync(modal);
        }
        [RateLimit(10)]
        [ModalInteraction("clan.start:*", true)]
        public async Task add(ulong _channel, ClanForm form)
        {
            await DeferAsync(true);

            var channel = Context.Guild.GetTextChannel(_channel);

            var thread = await channel.CreateThreadAsync(form.Title1, ThreadType.PrivateThread, ThreadArchiveDuration.OneWeek, null, false, 0);

            var message = await thread.SendMessageAsync(
                $"# Start of {form.Title1} Clan\n" +
                $"- This is your clan, a private place **No Admin, No Moderator**.\n" +
                $" - Invite or remove people with ```/clans```" +
                $"\n<@&480954902005415937> --> **<@{Context.User.Id}>");

            await message.PinAsync();

            await FollowupAsync($"## Done\nYour clan created, click here: {thread.Mention}.", ephemeral: true);

        }
        public class ClanForm : IModal
        {
            public string Title => "Clan";

            [InputLabel("Name")]
            [RequiredInput(true)]
            [ModalTextInput("name", TextInputStyle.Short, "its a name for your clan.", 0, maxLength: 16, null)]
            public string Title1 { get; set; }


        }
        [SlashCommand("clans-add-member", "add a member to your clan")]
        public async Task addMember(IGuildUser _user)
        {
            await DeferAsync();

            if (Context.Interaction.Channel is IThreadChannel thread)
            {
                var msgs = await thread.GetPinnedMessagesAsync();

                var botPinedMessage = (RestMessage)msgs
                    .FirstOrDefault(a => a.Author.Id == Context.Client.CurrentUser.Id);

                if (botPinedMessage is null) return;

                var user = botPinedMessage.MentionedUsers
                    .FirstOrDefault();

                if (user is null) return;
                if (user.Id != Context.User.Id)
                {
                    await FollowupAsync("only clan owner can add new member.");
                    return;
                }
               await thread.AddUserAsync(_user);

            }

        }

        [SlashCommand("clans-remove-member", "remove a member from your clan")]
        public async Task removeMember(IGuildUser _user)
        {
            await DeferAsync();

            if (Context.Interaction.Channel is IThreadChannel thread)
            {
                var msgs = await thread.GetPinnedMessagesAsync();

                var botPinedMessage = (RestMessage)msgs
                    .FirstOrDefault(a => a.Author.Id == Context.Client.CurrentUser.Id);

                if (botPinedMessage is null) return;

                var user = botPinedMessage.MentionedUsers
                    .FirstOrDefault();

                if (user is null) return;
                if (user.Id != Context.User.Id)
                {
                    await FollowupAsync("only clan owner can remove members.");
                    return;
                }
                await thread.RemoveUserAsync(_user);

            }

        }

    }
}
