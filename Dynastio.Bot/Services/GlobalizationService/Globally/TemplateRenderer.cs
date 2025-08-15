using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services.GlobalizationService.Globally
{
    internal static class TemplateRenderer
    {
        // Matches {name} or {name:format}
        private static readonly Regex Token = new(@"\{(?<name>[\w\.\-]+)(:(?<fmt>[^}]+))?\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Render(string template, IReadOnlyDictionary<string, object> values, IFormatProvider provider)
        {
            return Token.Replace(template, m =>
            {
                var name = m.Groups["name"].Value;
                var fmt = m.Groups["fmt"].Success ? m.Groups["fmt"].Value : null;

                if (!values.TryGetValue(name, out var val) || val is null)
                    return m.Value;

                if (val is IFormattable formattable)
                    return formattable.ToString(fmt, provider);

                return val.ToString() ?? string.Empty;
            });
        }
    }
}
