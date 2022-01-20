// <copyright file="YahooHistoricalClient.Record.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

/// <summary>
/// Data records for the Yahoo historical client class.
/// </summary>
public partial class YahooHistoricalClient
{
    /// <summary>
    /// Gets date of price.
    /// </summary>
    public DateOnly Date
    {
        get { return DateOnly.FromDateTime(DateTime.Now); }
    }

    /// <summary>
    /// Gets opening price of the stock.
    /// </summary>
    public double? Open
    {
        get { return null; }
    }

    /// <summary>
    /// Gets the high price.
    /// </summary>
    public double? High
    {
        get { return null; }
    }

    /// <summary>
    /// Gets the low price.
    /// </summary>
    public double? Low
    {
        get { return null; }
    }

    /// <summary>
    /// Gets closing price.
    /// </summary>
    public double? Close
    {
        get { return null; }
    }

    /// <summary>
    /// Gets the adjusted closing price.
    /// </summary>
    public double? AdjClose
    {
        get { return null; }
    }

    /// <summary>
    /// Gets the volume traded.
    /// </summary>
    public long? Volume
    {
        get { return null; }
    }
}