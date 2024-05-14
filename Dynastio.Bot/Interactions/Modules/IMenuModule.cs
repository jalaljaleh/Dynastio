using Discord;
using Dynastio.Bot.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules
{

    public interface IMenuModule
    {

        public Task SlashCommandAsync();
        public Task ButtonAsync();
        public Task ExecuteAsync();
        string TryCreateTextContent();
        bool TryCreateEmbeds(out Embed[] embeds);
        bool TryCreateComponents(out ComponentBuilder component);
    }
}
