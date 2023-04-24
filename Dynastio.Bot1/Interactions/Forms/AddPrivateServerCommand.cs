using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class AddPrivateServerCommandForm : IModal
    {
        public string Title => "Add Command";

        [InputLabel("Command")]
        [RequiredInput(true)]
        [ModalTextInput("command", TextInputStyle.Short, "The command name", 1, maxLength: 50, null)]
        public string Command { get; set; }

        [InputLabel("Description")]
        [RequiredInput(true)]
        [ModalTextInput("description", TextInputStyle.Paragraph, "write something about this command.", 5, maxLength: 500, null)]
        public string Description { get; set; }

        [InputLabel("Example")]
        [RequiredInput(true)]
        [ModalTextInput("example", TextInputStyle.Short, "an example like: /cmd cometo Hadi", 5, maxLength: 100,null)]
        public string Example { get; set; }


    }
}
