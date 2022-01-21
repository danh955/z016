// <copyright file="StringExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi
{
    /// <summary>
    /// String extensions class.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Parse string into DateOnly value.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <returns>DateOnly.  Null if invalid.</returns>
        internal static DateOnly? GetDateOnly(this string text)
        {
            return DateOnly.TryParse(text, out DateOnly date)
                ? date
                : null;
        }

        /// <summary>
        /// Parse string into double value.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <returns>Double value.  Null if invalid.</returns>
        internal static double? GetDouble(this string text)
        {
            return double.TryParse(text, out double value)
                ? value
                : null;
        }

        /// <summary>
        /// Parse string into long value.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <returns>Long value.  Null if invalid.</returns>
        internal static long? GetLong(this string text)
        {
            return long.TryParse(text, out long value)
                ? value
                : null;
        }
    }
}