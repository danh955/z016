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

    /// <summary>
    /// Gets a value indicating whether parsing was suspenseful.
    /// </summary>
    internal bool IsSuccess { get; private set; }

    /// <summary>
    /// Sets text line to parse.
    /// </summary>
    internal string Line
    {
        set
        {
            this.column = value.Split(',');
            if (this.column.Length == 7)
            {
                var date = this.column[0].GetDateOnly();
                if (date != null)
                {
                    this.Date = date.Value;
                    this.IsSuccess = true;
                }
                else
                {
                    this.logger?.LogDebug("Yahoo has an invalid date.\n {data}", value);
                    this.IsSuccess = false;
                }
            }
            else
            {
                this.logger?.LogDebug("Yahoo did not return 7 columns.\n {data}", value);
                this.IsSuccess = false;
            }
        }
    }
}