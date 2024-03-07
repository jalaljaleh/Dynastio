using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Modals.Forms
{
    public class SearchPlayerModalForm : IModal
    {
        public string Title => "Search";

        [InputLabel("Player Nickname")]
        [RequiredInput(false)]
        [ModalTextInput("nickname", TextInputStyle.Short, "Search by Player Nickname", 0, maxLength: 16, null)]
        public string PlayerNickname { get; set; }

        [InputLabel("Player Level")]
        [RequiredInput(false)]
        [ModalTextInput("level", TextInputStyle.Short, "0-25", 0, maxLength: 16, null)]
        public string PlayerLevel { get; set; }

        [InputLabel("Player Score")]
        [RequiredInput(false)]
        [ModalTextInput("score", TextInputStyle.Short, "0-10000", 0, maxLength: 16, null)]
        public string PlayerScore { get; set; }

        [InputLabel("Team")]
        [RequiredInput(false)]
        [ModalTextInput("team", TextInputStyle.Short, "Team name", 0, maxLength: 16, null)]
        public string Team { get; set; }

        [InputLabel("Server")]
        [RequiredInput(false)]
        [ModalTextInput("server", TextInputStyle.Short, "Server name", 0, maxLength: 16, null)]
        public string Server { get; set; }
    }
}
