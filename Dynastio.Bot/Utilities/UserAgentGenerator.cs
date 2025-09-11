using System;
using System.Linq;

public static class UserAgentGenerator
{
    private static readonly Random _rnd = new Random();

    // Common platform tokens
    private static readonly string[] Platforms = new[]
    {
        "Windows NT 10.0; Win64; x64",
        "Windows NT 10.0; WOW64",
        "Windows NT 6.1; Win64; x64",
        "Macintosh; Intel Mac OS X 10_15_7",
        "X11; Linux x86_64",
        "Android 11; Mobile",
        "Android 12; Mobile",
        "iPhone; CPU iPhone OS 14_0 like Mac OS X",
        "iPad; CPU OS 14_0 like Mac OS X"
    };

    // Browser version formats
    private static readonly Func<string>[] BrowserFormats = new Func<string>[]
    {
        () => $"Chrome/{Version(70, 99)}.{Version(0, 9999)}.{Version(0, 999)}",
        () => $"Firefox/{Version(60, 100)}.0",
        () => $"Safari/{Version(13, 15)}.0",
        () => $"Edge/{Version(80, 115)}.{Version(0, 999)}"
    };

    // WebKit / Gecko engine fragments
    private static readonly string[] EngineFragments = new[]
    {
        "AppleWebKit/537.36 (KHTML, like Gecko)",
        "Gecko/20100101"
    };

    /// <summary>
    /// Generates a random User-Agent string combining platform, engine, and browser tokens.
    /// </summary>
    public static string GenerateUserAgent()
    {
        // Pick random components
        var platform = Platforms[_rnd.Next(Platforms.Length)];
        var engine = EngineFragments[_rnd.Next(EngineFragments.Length)];
        var browser = BrowserFormats[_rnd.Next(BrowserFormats.Length)]();

        // Assemble final UA
        return $"Mozilla/5.0 ({platform}) {engine} {browser}";
    }

    /// <summary>
    /// Helper to generate a random integer in [min, max].
    /// </summary>
    private static int Version(int min, int max)
    {
        return _rnd.Next(min, max + 1);
    }
}
