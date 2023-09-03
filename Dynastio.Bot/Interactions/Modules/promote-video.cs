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
using Dynastio.Bot.Interactions.AutoCompeletes;
using Newtonsoft.Json;

namespace Dynastio.Bot.Interactions.Modules
{
    [EnabledInDm(false)]
    public class promotevideoModule : CustomInteractionModuleBase
    {
        public UserService _userService { get; set; }
        public InternetService _internetService { get; set; }
        public DiscordSocketClient _discord { get; set; }
        public IDynastioBotDatabase _database { get; set; }

            //[DefaultMemberPermissions(GuildPermission.Administrator)]
            //[RequireRole(480954902005415937)]
            //[SlashCommand("promote-video", "promote your dynastio video !")]
            //public async Task promote(string url)
            //{
            //    await DeferAsync();
            //    if (string.IsNullOrEmpty(BotUser.youtube_channel))
            //    {
            //        await 
            //        return;
            //    }
        
            //}



    }
}
