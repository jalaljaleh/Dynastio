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

namespace Dynastio.Bot.Interactions.Modules.Owner
{

    [RequireBotOwner]
    [EnabledInDm(true)]
    [Group("owner", "config guilds")]
    public class OwnerModule : CustomInteractionModuleBase<CustomSocketInteractionContext>
    {
        public GuildService _guildService { get; set; }
        public UserService _userService { get; set; }


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
            [SlashCommand("set", "mark this guild as official guild")]
            public async Task set()
            {
                await DeferAsync();
                await _guildService.SetOfficialGuildAsync(Context.Guild.Id);
                await FollowupAsync(true ? "done, the official server is this guild now !" : "There is a problem.");
            }

        }
    }
}
