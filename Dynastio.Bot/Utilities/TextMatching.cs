using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dynastio.Bot
{
    public static class TextMatching
    {
        // ────────────── NORMALIZATION ──────────────

        /// <summary>Lowercases, trims and strips diacritics.</summary>
        public static string Normalize(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Decompose & remove non-spacing marks
            var normalized = input
                .Trim()
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ────────────── LEVENSHTEIN & DAMERAU ──────────────

        /// <summary>Classic Levenshtein distance.</summary>
        public static int LevenshteinDistance(this string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[n, m];
        }

        /// <summary>Damerau–Levenshtein distance (includes transpositions).</summary>
        public static int DamerauLevenshtein(this string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 2, m + 2];
            int maxdist = n + m;
            d[0, 0] = maxdist;
            for (int i = 0; i <= n; i++)
            {
                d[i + 1, 0] = maxdist;
                d[i + 1, 1] = i;
            }
            for (int j = 0; j <= m; j++)
            {
                d[0, j + 1] = maxdist;
                d[1, j + 1] = j;
            }

            var da = new Dictionary<char, int>();
            foreach (var ch in (s + t)) da[ch] = 0;

            for (int i = 1; i <= n; i++)
            {
                int db = 0;
                for (int j = 1; j <= m; j++)
                {
                    int i1 = da[t[j - 1]];
                    int j1 = db;
                    int cost = s[i - 1] == t[j - 1] ? (db = j) - j1 : 1;

                    d[i + 1, j + 1] = Math.Min(
                        Math.Min(
                            d[i, j] + cost,      // substitution
                            d[i + 1, j] + 1       // insertion
                        ),
                        Math.Min(
                            d[i, j + 1] + 1,     // deletion
                            d[i1, j1] + (i - i1 - 1) + 1 + (j - j1 - 1)
                        )
                    );
                }
                da[s[i - 1]] = i;
            }

            return d[n + 1, m + 1];
        }

        // ────────────── JARO & JARO–WINKLER ──────────────

        /// <summary>Returns Jaro similarity [0…1].</summary>
        public static double Jaro(this string s1, string s2)
        {
            s1 = s1.Normalize();
            s2 = s2.Normalize();
            if (s1 == s2) return 1.0;

            int len1 = s1.Length, len2 = s2.Length;
            if (len1 == 0 || len2 == 0) return 0.0;

            int matchDistance = Math.Max(len1, len2) / 2 - 1;
            var s1Matches = new bool[len1];
            var s2Matches = new bool[len2];

            int matches = 0, transpositions = 0;
            // Find matches
            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance, len2 - 1);
                for (int j = start; j <= end; j++)
                {
                    if (s2Matches[j]) continue;
                    if (s1[i] != s2[j]) continue;
                    s1Matches[i] = s2Matches[j] = true;
                    matches++;
                    break;
                }
            }
            if (matches == 0) return 0.0;

            // Count transpositions
            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[k]) k++;
                if (s1[i] != s2[k]) transpositions++;
                k++;
            }

            double m = matches;
            return (
                (m / len1) +
                (m / len2) +
                ((m - transpositions / 2.0) / m)
            ) / 3.0;
        }

        /// <summary>Returns Jaro–Winkler similarity [0…1].</summary>
        public static double JaroWinkler(this string s1, string s2, double prefixScale = 0.1, int maxPrefix = 4)
        {
            double jaroSim = s1.Jaro(s2);
            // common prefix length up to maxPrefix
            int prefixLen = 0;
            for (; prefixLen < Math.Min(Math.Min(s1.Length, s2.Length), maxPrefix); prefixLen++)
                if (s1[prefixLen] != s2[prefixLen]) break;

            return jaroSim + prefixLen * prefixScale * (1 - jaroSim);
        }

        // ────────────── PHONETIC CODING (SOUNDEX) ──────────────

        /// <summary>Generates US-English Soundex code (4 chars).</summary>
        public static string Soundex(this string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            name = name.Normalize().ToUpperInvariant();
            var firstLetter = name[0];

            // Map letters to digits
            var map = new Dictionary<char, char>
            {
                {'B','1'},{'F','1'},{'P','1'},{'V','1'},
                {'C','2'},{'G','2'},{'J','2'},{'K','2'},{'Q','2'},{'S','2'},{'X','2'},{'Z','2'},
                {'D','3'},{'T','3'},
                {'L','4'},
                {'M','5'},{'N','5'},
                {'R','6'}
            };

            var sb = new StringBuilder().Append(firstLetter);
            char lastCode = map.ContainsKey(firstLetter) ? map[firstLetter] : '0';

            // Encode rest
            foreach (var c in name.Skip(1))
            {
                if (!map.TryGetValue(c, out var code)) code = '0';
                if (code == lastCode) continue;
                if (code != '0') sb.Append(code);
                lastCode = code;
                if (sb.Length == 4) break;
            }

            // Pad with zeros
            return sb.ToString().PadRight(4, '0');
        }

        // ────────────── CONVENIENCE METHODS ──────────────

        /// <summary>
        /// Returns true if two strings are sufficiently similar by chosen algorithm.
        /// </summary>
        public static bool IsMatch(
            this string input,
            string pattern,
            double threshold = 0.8,
            MatchAlgorithm algorithm = MatchAlgorithm.JaroWinkler)
        {
            input = input.Normalize();
            pattern = pattern.Normalize();

            double score = algorithm switch
            {
                MatchAlgorithm.Levenshtein => 1.0 - input.LevenshteinDistance(pattern) / (double)Math.Max(input.Length, pattern.Length),
                MatchAlgorithm.Damerau => 1.0 - input.DamerauLevenshtein(pattern) / (double)Math.Max(input.Length, pattern.Length),
                MatchAlgorithm.Jaro => input.Jaro(pattern),
                _ => input.JaroWinkler(pattern)
            };

            return score >= threshold;
        }

        /// <summary>Normalized similarity [0…1].</summary>
        public static double GetSimilarity(
            this string input,
            string pattern,
            MatchAlgorithm algorithm = MatchAlgorithm.JaroWinkler)
        {
            input = input.Normalize();
            pattern = pattern.Normalize();

            return algorithm switch
            {
                MatchAlgorithm.Levenshtein => 1.0 - input.LevenshteinDistance(pattern) / (double)Math.Max(input.Length, pattern.Length),
                MatchAlgorithm.Damerau => 1.0 - input.DamerauLevenshtein(pattern) / (double)Math.Max(input.Length, pattern.Length),
                MatchAlgorithm.Jaro => input.Jaro(pattern),
                _ => input.JaroWinkler(pattern)
            };
        }

        /// <summary>Finds best matching string in a list along with its score.</summary>
        public static (string Match, double Score) FindBestMatch(
            this IEnumerable<string> candidates,
            string pattern,
            MatchAlgorithm algorithm = MatchAlgorithm.JaroWinkler)
        {
            var normalized = pattern.Normalize();
            var best = candidates
                .Select(s => new { Text = s, Score = s.Normalize().GetSimilarity(normalized, algorithm) })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            return best == null ? (null, 0.0) : (best.Text, best.Score);
        }

        /// <summary>
        /// Checks if any substring of input fuzzy-matches pattern
        /// using Levenshtein at distance &lt;= maxDistance.
        /// </summary>
        public static bool FuzzyContains(this string input, string pattern, int maxDistance = 2)
        {
            input = input.Normalize();
            pattern = pattern.Normalize();
            int plen = pattern.Length;
            for (int i = 0; i + plen <= input.Length; i++)
            {
                var slice = input.Substring(i, plen);
                if (slice.LevenshteinDistance(pattern) <= maxDistance)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Available matching algorithms.</summary>
    public enum MatchAlgorithm
    {
        Levenshtein,
        Damerau,
        Jaro,
        JaroWinkler
    }
}



