using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Forms
{
    public class AddAccountModalForm : IModal
    {
        public string Title => "Add Account";

        [InputLabel("Account Id")]
        [RequiredInput(true)]
        [ModalTextInput("id", TextInputStyle.Short, "google:0000000000000000000", 0, maxLength: 150, null)]
        public string Id { get; set; }

        [InputLabel("Pin Code")]
        [RequiredInput(true)]
        [ModalTextInput("pincode", TextInputStyle.Short, "Your account pin code", 0, maxLength: 16, null)]
        public string PinCode { get; set; }

        [InputLabel("Reminder")]
        [RequiredInput(true)]
        [ModalTextInput("reminder", TextInputStyle.Short, "its a field that you can write anything.", 0, maxLength: 60, null)]
        public string Reminder { get; set; }

        [InputLabel("Email")]
        [RequiredInput(true)]
        [ModalTextInput("email", TextInputStyle.Short, "Gmail, Discord, Facebook, Youtube Channel Address.", 0, maxLength: 100, null)]
        public string Email { get; set; }

    }
}
