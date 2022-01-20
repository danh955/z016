// <copyright file="HelperExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System;

/// <summary>
/// For parsing data.
/// </summary>
internal static class HelperExtensions
{
    /// <summary>
    /// Convert date to Unix time stamp.
    /// </summary>
    /// <param name="date">Date to convert.</param>
    /// <returns>Unix time stamp in string form.</returns>
    internal static string ToUnixTimestamp(this DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue).ToUnixTimestamp();

    /// <summary>
    /// Convert date time to Unix time stamp.
    /// </summary>
    /// <param name="date">Date to convert.</param>
    /// <returns>Unix time stamp in string form.</returns>
    internal static string ToUnixTimestamp(this DateTime date) =>
        DateTime.SpecifyKind(date.FromEstToUtc(), DateTimeKind.Utc)
            .Subtract(Constant.EpochDate)
            .TotalSeconds
            .ToString();

    private static DateTime FromEstToUtc(this DateTime date) =>
        DateTime.SpecifyKind(date, DateTimeKind.Unspecified)
            .ToUtcFrom(Constant.EasternTimeZone);

    private static DateTime ToUtcFrom(this DateTime date, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeToUtc(date, timeZone);
}