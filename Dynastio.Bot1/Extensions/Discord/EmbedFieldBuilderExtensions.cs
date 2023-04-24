using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal static class EmbedFieldBuilderExtensions
    {
        public static List<EmbedFieldBuilder> AddField(this List<EmbedFieldBuilder> List, string Name, string Value, bool IsInline = true)
        {
            List.Add(new EmbedFieldBuilder() { IsInline = IsInline, Name = Name, Value = Value });
            return List;
        }
    }
}
