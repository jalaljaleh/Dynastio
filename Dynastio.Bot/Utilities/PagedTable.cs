using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dynastio.Bot
{
    /// <summary>
    /// Represents the result of creating a paged table, including rendered text and paging metadata.
    /// </summary>
    public class PagedTableResult
    {
        public string TableText { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    /// <summary>
    /// Provides helper methods for rendering collections as formatted tables with optional paging.
    /// </summary>
    public static class PagedTable
    {
        // ──────────────── PAGING WRAPPER ────────────────

        /// <summary>
        /// Converts a sequence into a formatted table with paging support.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="headers">Column headers.</param>
        /// <param name="selectors">Column value selectors.</param>
        /// <returns>A <see cref="PagedTableResult"/> containing table text and paging info.</returns>
        public static PagedTableResult ToPagedTable<T>(
            this IEnumerable<T> source,
            int page,
            int pageSize,
            string[] headers,
            params Func<T, object>[] selectors)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var items = source.ToList();

            // Ensure valid paging
            if (pageSize < 1) pageSize = 1;
            if (page < 1) page = 1;

            int totalItems = items.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            // Slice the data for the requested page
            var pagedItems = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return new PagedTableResult
            {
                TableText = pagedItems.ToFormattedTable(headers, selectors),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };
        }

        // ──────────────── TABLE RENDERING ────────────────


        /// <summary>
        /// Converts a sequence into a formatted table using read-only header & selector lists.
        /// </summary>
        public static string ToFormattedTable<T>(
            this IEnumerable<T> source,
            IReadOnlyList<string> headers,
            IReadOnlyList<Func<T, object>> selectors,
            string separator = "  ")
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (selectors == null) throw new ArgumentNullException(nameof(selectors));
            if (headers.Count != selectors.Count)
                throw new ArgumentException("Headers and selectors count must match.");

            // Build table rows: First row is header, followed by data rows
            var rows = new List<string[]> { headers.ToArray() };
            rows.AddRange(source.Select(item =>
                selectors.Select(sel => sel(item)?.ToString()?.Trim() ?? string.Empty).ToArray()
            ));

            return RenderTextTable(rows, separator);
        }

        /// <summary>
        /// Converts a sequence into a formatted table where headers are derived from expression names.
        /// </summary>
        public static string ToFormattedTable<T>(
            this IEnumerable<T> source,
            params Expression<Func<T, object>>[] selectors)
        {
            if (selectors == null || selectors.Length == 0)
                throw new ArgumentException("At least one selector is required.", nameof(selectors));

            // Extract property names for headers
            var headers = selectors
                .Select(expr => GetMemberName(expr) ?? string.Empty)
                .ToArray();

            // Compile selectors into executable functions
            var funcs = selectors.Select(expr => expr.Compile()).ToArray();

            return source.ToFormattedTable(headers, funcs);
        }

        // ──────────────── INTERNAL HELPERS ────────────────

        /// <summary>
        /// Renders a 2D list of strings as a text table with alignment.
        /// </summary>
        private static string RenderTextTable(List<string[]> rows, string separator)
        {
            int cols = rows[0].Length;

            // Determine maximum width of each column
            var widths = Enumerable.Range(0, cols)
                .Select(i => rows.Max(r => r[i]?.Length ?? 0))
                .ToArray();

            var sb = new StringBuilder();

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];

                for (int c = 0; c < cols; c++)
                {
                    // Right-align numbers in non-header rows
                    bool rightAlign = rowIndex > 0 && decimal.TryParse(row[c], out _);
                    string cell = row[c] ?? string.Empty;

                    sb.Append(rightAlign ? cell.PadLeft(widths[c]) : cell.PadRight(widths[c]));

                    if (c < cols - 1)
                        sb.Append(separator);
                }

                sb.AppendLine();

                // After headers, insert separator line
                if (rowIndex == 0)
                {
                    sb.AppendLine(string.Join(separator,
                        widths.Select(w => new string('-', w))));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of the member (property/field) targeted by the expression.
        /// </summary>
        private static string GetMemberName<T>(Expression<Func<T, object>> expr)
        {
            return expr.Body switch
            {
                UnaryExpression ux when ux.Operand is MemberExpression mx => mx.Member.Name,
                MemberExpression m => m.Member.Name,
                _ => null
            };
        }
    }
}
