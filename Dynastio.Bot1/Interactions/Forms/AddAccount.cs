using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class AddAccountForm : IModal
    {
        public string Title => "Add Account";

        [InputLabel("Nickname")]
        [RequiredInput(true)]
        [ModalTextInput("nickname", TextInputStyle.Short, "Custom Nickname", 0, maxLength: 16, null)]
        public string Nickname { get; set; }

        [InputLabel("Coins")]
        [RequiredInput(true)]
        [ModalTextInput("coin", TextInputStyle.Short, "Your account coins number", 0, maxLength: 16, null)]
        public string Coins { get; set; }

        [InputLabel("Account Id")]
        [RequiredInput(true)]
        [ModalTextInput("id", TextInputStyle.Short, "google:0000000000000000000", 0, maxLength: 150, null)]
        public string Id { get; set; }

        [InputLabel("Reminder")]
        [RequiredInput(true)]
        [ModalTextInput("reminder", TextInputStyle.Paragraph, "its a private field you can write anything.", 0, maxLength: 500,null)]
        public string Reminder { get; set; }

        [InputLabel("Description")]
        [RequiredInput(false)]
        [ModalTextInput("description", TextInputStyle.Short, "write something about this account.", 0, maxLength: 50, null)]
        public string Description { get; set; }
    }
}
