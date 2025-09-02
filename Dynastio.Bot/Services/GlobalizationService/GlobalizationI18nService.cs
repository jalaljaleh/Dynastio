using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using Dynastio.Bot.Services.GlobalizationService.Globally;

namespace Dynastio.Bot
{
    public static class GlobalizationI18nService
    {
        private static readonly ResourceManager Rm = GloballyLocale.ResourceManager;
        private static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en");
        private static readonly AsyncLocal<CultureInfo?> AmbientCulture = new();

        // Set default culture at startup if needed
        public static void SetDefaultCulture(string cultureName)
        {
            try { CultureInfo.GetCultureInfo(cultureName); }
            catch { cultureName = "en"; }
            typeof(GlobalizationI18nService).GetField("<DefaultCulture>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }

        // Use a culture for the current async flow (auto-reverts on dispose)
        public static IDisposable Use(string cultureName) => new CultureScope(cultureName);

        // Current culture in scope (falls back to default)
        private static CultureInfo Current => AmbientCulture.Value ?? DefaultCulture;

        // Positional formatting: key -> "Welcome, {0}"
        public static string Get(string key, params object?[] args)
            => GetCore(key, Current, args);

        // Culture-explicit version
        public static string Get(string key, CultureInfo culture, params object?[] args)
            => GetCore(key, culture, args);

        // Named placeholders: key -> "Welcome, {user}! Today is {date:dddd}"
        public static string Format(string key, object? model)
            => FormatCore(key, Current, model);

        public static string Format(string key, CultureInfo culture, object? model)
            => FormatCore(key, culture, model);

        private static string GetCore(string key, CultureInfo culture, params object?[] args)
        {
            var s = Rm.GetString(key, culture) ?? key;
            if (args is { Length: > 0 })
            {
                try { return string.Format(culture, s, args); }
                catch { /* ignore formatting errors */ }
            }
            return s;
        }

        private static string FormatCore(string key, CultureInfo culture, object? model)
        {
            var template = Rm.GetString(key, culture) ?? key;
            if (model is null) return template;

            var dict = ToDictionary(model);
            return TemplateRenderer.Render(template, dict, culture);
        }

        private static IReadOnlyDictionary<string, object?> ToDictionary(object model)
        {
            if (model is IReadOnlyDictionary<string, object?> ro) return ro;
            if (model is IDictionary<string, object?> d) return new Dictionary<string, object?>(d);

            var map = new Dictionary<string, object?>(StringComparer.Ordinal);
            var props = model.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var p in props) if (p.CanRead) map[p.Name] = p.GetValue(model);
            return map;
        }

        private sealed class CultureScope : IDisposable
        {
            private readonly CultureInfo? _prev;
            public CultureScope(string cultureName)
            {
                _prev = AmbientCulture.Value;
                AmbientCulture.Value = Safe(cultureName);
            }
            public void Dispose() => AmbientCulture.Value = _prev;

            private static CultureInfo Safe(string name)
            {
                try { return CultureInfo.GetCultureInfo(name); }
                catch { return DefaultCulture; }
            }
        }
    }

   


}
