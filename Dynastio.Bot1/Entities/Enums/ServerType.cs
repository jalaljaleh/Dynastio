using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{

    public enum FilterType
    {
        [ChoiceDisplay("Private Servers")]
        PrivateServer,
        [ChoiceDisplay("Public Servers")]
        PublicServer,
        [ChoiceDisplay("All Servers")]
        All
    }
    public enum SortType
    {
        [ChoiceDisplay("Player Score")]
        Score,
        [ChoiceDisplay("Player Level")]
        Level,
        [ChoiceDisplay("Player Team Name")]
        Team,
        [ChoiceDisplay("Server Name")]
        Server,
        [ChoiceDisplay("Player Nickname")]
        Nickname,
        [ChoiceDisplay("Server Region")]
        Region,
    }

    public enum Map
    {
        [ChoiceDisplay("Display")]
        Enable,
        Disable
    }
}
