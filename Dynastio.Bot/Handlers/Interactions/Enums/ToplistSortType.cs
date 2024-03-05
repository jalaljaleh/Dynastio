using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers.Interactions
{
    public enum ToplistSortType
    {
        Score,
        Level,
        Nickname,
        Team,
        Location,
        [ChoiceDisplay("Server Name")]
        ServerName
    }


}
