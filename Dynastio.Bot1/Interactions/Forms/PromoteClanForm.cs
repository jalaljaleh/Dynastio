using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class PromoteClanForm : IModal
    {
        public string Title => "Promote Clan";

        [InputLabel("Server Id")]
        [RequiredInput(true)]
        [ModalTextInput("id", TextInputStyle.Short, "Your server id", 0, maxLength: 50, null)]
        public string Id { get; set; }

        [InputLabel("Clan Name")]
        [RequiredInput(true)]
        [ModalTextInput("name", TextInputStyle.Short, "Your clan Name", 0, maxLength: 16, null)]
        public string ClanName { get; set; }

        [InputLabel("Description")]
        [RequiredInput(true)]
        [ModalTextInput("description", TextInputStyle.Paragraph, "description", 0, maxLength: 500, null)]
        public string Description { get; set; }

        [InputLabel("Region")]
        [RequiredInput(true)]
        [ModalTextInput("region", TextInputStyle.Short, "Region include language", 0, maxLength: 20, "English")]
        public string Region { get; set; }

        [InputLabel("Terms and conditions")]
        [RequiredInput(true)]
        [ModalTextInput("Terms", TextInputStyle.Paragraph, "Terms and conditions", 0, maxLength: 1000, null)]
        public string Terms { get; set; }
    }
}
