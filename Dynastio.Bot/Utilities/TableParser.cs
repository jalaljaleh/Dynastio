using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dynastio.Bot
{
    public static class TableFormatter
    {
        /// <summary>
        /// Formats a sequence of records into an aligned text table.
        /// Headers are auto-derived from expressions if none supplied.
        /// </summary>
        public static string ToTable<T>(
            this IEnumerable<T> source,
            string[] headers,
            Func<T, object>[] selectors,
            string separator = "  ")
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));
            if (selectors is null) throw new ArgumentNullException(nameof(selectors));
            if (headers.Length != selectors.Length)
                throw new ArgumentException("Headers and selectors count must match.");

            // Build rows: first header, then each value row
            var rows = new List<string[]>
            {
                headers
            };

            rows.AddRange(
                source.Select(item =>
                    selectors.Select(sel => sel(item)?.ToString() ?? string.Empty).ToArray()
                )
            );

            return BuildTable(rows, separator);
        }

        /// <summary>
        /// Formats a sequence of records into an aligned text table.
        /// Derives headers from the names of the supplied expressions.
        /// </summary>
        public static string ToTable<T>(
            this IEnumerable<T> source,
            params Expression<Func<T, object>>[] selectors)
        {
            if (selectors is null || selectors.Length == 0)
                throw new ArgumentException("At least one selector is required.", nameof(selectors));

            // Derive headers from member names
            var headers = selectors
                .Select(expr => GetMemberName(expr) ?? string.Empty)
                .ToArray();

            var funcs = selectors
                .Select(expr => expr.Compile())
                .ToArray();

            return source.ToTable(headers, funcs);
        }

        // ──────────────── PRIVATE HELPERS ────────────────

        private static string BuildTable(List<string[]> rows, string separator)
        {
            // Calculate max width per column
            int cols = rows[0].Length;
            var widths = new int[cols];

            foreach (var row in rows)
            {
                for (int c = 0; c < cols; c++)
                {
                    widths[c] = Math.Max(widths[c], row[c]?.Length ?? 0);
                }
            }

            // Assemble each row
            var sb = new StringBuilder();
            foreach (var row in rows)
            {
                for (int c = 0; c < cols; c++)
                {
                    string cell = row[c] ?? string.Empty;
                    sb.Append(cell.PadRight(widths[c]));
                    if (c < cols - 1)
                        sb.Append(separator);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetMemberName<T>(Expression<Func<T, object>> expr)
        {
            // Handle conversions (value types to object)
            if (expr.Body is UnaryExpression ux && ux.Operand is MemberExpression mx)
                return mx.Member.Name;

            if (expr.Body is MemberExpression m)
                return m.Member.Name;

            return null;
        }
    }
}