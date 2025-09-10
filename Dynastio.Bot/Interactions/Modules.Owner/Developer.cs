using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Owner
{
    [RequireTeam]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    [Group("developer", "developer commands")]
    public class Developer : MenuModulesBase
    {
        [SlashCommand("pincode", "bypass pin code")]
        public async Task ByPassPinCode(string newCode)
        {
            Interactions.Modules.Menu.Buttons.ButtonLoginModule.BypassPinCode = newCode;
            await RespondAsync($"PinCode-bypass created successfuly.", ephemeral: true);
        }
    }
}
