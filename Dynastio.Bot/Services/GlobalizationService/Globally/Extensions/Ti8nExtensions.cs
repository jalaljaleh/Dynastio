
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services.GlobalizationService.Globally.Extensions
{
    // Type-less sugar for your code: "key".T(), "key".Tf(...)
    public static class I18nExtensions
    {
        // Positional
        public static string T(this string key, params object?[] args)
            => GlobalizationI18nService.Get(key, args);

        // Named placeholders
        public static string Tf(this string key, object? model)
            => GlobalizationI18nService.Format(key, model);

        // One-off culture (no scope)
        public static string Tin(this string key, string culture, params object?[] args)
            => GlobalizationI18nService.Get(key, CultureInfo.GetCultureInfo(culture), args);

        public static string Tfin(this string key, string culture, object? model)
            => GlobalizationI18nService.Format(key, CultureInfo.GetCultureInfo(culture), model);
    }
}
