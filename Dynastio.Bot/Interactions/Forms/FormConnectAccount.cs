using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Forms
{
    public class FormConnectAccount : IModal
    {
        public string Title => "Connect Account";

        [InputLabel("Account Id")]
        [RequiredInput(true)]
        [ModalTextInput("id", TextInputStyle.Short, "youtube:00000000000", 0, maxLength: 150, null)]
        public string Id { get; set; }

        [InputLabel("Pin Code")]
        [RequiredInput(true)]
        [ModalTextInput("pincode", TextInputStyle.Short, "Your account pin code", 0, maxLength: 16, null)]
        public string PinCode { get; set; }

    }
}
