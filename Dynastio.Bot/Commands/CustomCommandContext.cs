using Discord;
using Discord.Commands;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynastio.Bot
{
    public class CustomCommandContext : CommandContext
    {


        public CustomCommandContext(IDiscordClient client, IUserMessage msg) : base(client, msg)
        {

        }

       
    }
}
