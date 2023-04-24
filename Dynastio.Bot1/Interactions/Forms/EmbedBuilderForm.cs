using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class EmbedBuilderForm : IModal
    {
        public string Title => "Create Embed";

        [InputLabel("Content")]
        [RequiredInput(false)]
        [ModalTextInput("content", TextInputStyle.Paragraph, "message content", 0, maxLength: 2000)]
        public string Content { get; set; }

        [InputLabel("Title")]
        [RequiredInput(false)]
        [ModalTextInput("title", TextInputStyle.Short, "Write a title", 0, maxLength: 256, null)]
        public string Title_ { get; set; }

        [InputLabel("Description")]
        [RequiredInput(false)]
        [ModalTextInput("description", TextInputStyle.Paragraph, "Write a description", 0, maxLength: 4000, null)]
        public string Description { get; set; }

        [InputLabel("Thumbnail Url")]
        [RequiredInput(false)]
        [ModalTextInput("thumbnail-url", TextInputStyle.Short, "upload image in a channel and copy its url link", 0, maxLength: 200, null)]
        public string ThumbnailUrl { get; set; }

        [InputLabel("Image Url")]
        [RequiredInput(false)]
        [ModalTextInput("image-url", TextInputStyle.Short, "upload image in a channel and copy its url link", 0, maxLength: 200, null)]
        public string ImageUrl { get; set; }
    }
}
