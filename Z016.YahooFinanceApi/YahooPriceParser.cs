// <copyright file="YahooPriceParser.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using Microsoft.Extensions.Logging;

/// <summary>
/// Yahoo price parser class.
/// </summary>
public class YahooPriceParser
{
    private readonly ILogger? logger;
    private string[] column;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooPriceParser"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    internal YahooPriceParser(ILogger? logger = default)
    {
        this.logger = logger;
        this.column = Array.Empty<string>();
    }

    /// <summary>
    /// Gets the date.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the opening price.
    /// </summary>
    public double? Open => this.column[1].GetDouble();

    /// <summary>
    /// Gets the high price.
    /// </summary>
    public double? High => this.column[2].GetDouble();

    /// <summary>
    /// Gets the low price.
    /// </summary>
    public double? Low => this.column[3].GetDouble();

    /// <summary>
    /// Gets the closing price.
    /// </summary>
    public double? Close => this.column[4].GetDouble();

    /// <summary>
    /// Gets the adjusted closing price.
    /// </summary>
    public double? AdjClose => this.column[5].GetDouble();

    /// <summary>
    /// Gets the volume.
    /// </summary>
    public long? Volume => this.column[6].GetLong();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(YahooPriceParser)} {{ Date = {this.Date}, Open = {this.Open}, High = {this.High}, Low = {this.Low}, Close = {this.Close}, AdjClose = {this.AdjClose}, Volume = {this.Volume} }}";
    }

    /// <summary>
    /// Split the line of text into columns.
    /// </summary>
    /// <param name="line">String to parse.</param>
    /// <returns>True if successful.</returns>
    internal bool SplitLine(string line)
    {
        this.column = line.Split(',');
        if (this.column.Length == 7)
        {
            var date = this.column[0].GetDateOnly();
            if (date != null)
            {
                this.Date = date.Value;
                return true;
            }
            else
            {
                this.logger?.LogDebug("Yahoo has an invalid date.\n {data}", line);
                return false;
            }
        }
        else
        {
            this.logger?.LogDebug("Yahoo did not return 7 columns.\n {data}", line);
            return false;
        }
    }
}