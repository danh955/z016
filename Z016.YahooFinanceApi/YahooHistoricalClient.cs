// <copyright file="YahooHistoricalClient.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using Microsoft.Extensions.Logging;

/// <summary>
/// Yahoo historical stock data client class.
/// </summary>
public class YahooHistoricalClient : YahooClientBase
{
    private string symbol;
    private DateOnly? firstDate;
    private DateOnly? lastDate;
    private YahooInterval? interval;
    private ILogger<YahooHistoricalClient>? logger;
    private CancellationToken cancellationToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooHistoricalClient"/> class.
    /// </summary>
    /// <param name="symbol">Symbol of prices to get.</param>
    /// <param name="firstDate">First date.</param>
    /// <param name="lastDate">Last date.</param>
    /// <param name="interval">Stock price interval.</param>
    /// <param name="logger">ILogger.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    public YahooHistoricalClient(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        ILogger<YahooHistoricalClient>? logger = null,
        CancellationToken? cancellationToken = null)
    {
        this.symbol = symbol;
        this.firstDate = firstDate;
        this.lastDate = lastDate;
        this.interval = interval;
        this.logger = logger;
        this.cancellationToken = cancellationToken ?? CancellationToken.None;

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentNullException(nameof(symbol));
        }

        if (firstDate != null && lastDate != null)
        {
            if (firstDate > lastDate)
            {
                throw new ArgumentException($"{nameof(firstDate)} is grater then {nameof(lastDate)}");
            }
        }
    }

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

    /// <summary>
    /// Get the next row.
    /// </summary>
    /// <returns>True if no more rows.</returns>
    public async Task<bool> Next()
    {
        await this.CheckFirstTime();
        return true;
    }

    /// <summary>
    /// Get CSV from website.
    /// </summary>
    /// <returns>Task.</returns>
    private async Task CheckFirstTime()
    {
        //// TODO: is this the first time?

        string period1 = this.firstDate.HasValue ? this.firstDate.Value.ToUnixTimestamp() : Constant.EpochString;
        string period2 = this.lastDate.HasValue ? this.lastDate.Value.ToUnixTimestamp() : DateTime.Today.ToUnixTimestamp();
        string intervalString = ToIntervalString(this.interval);

        await this.CheckCrumb(this.cancellationToken);
        string url = $"https://query1.finance.yahoo.com/v7/finance/download/{this.symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={Crumb}";

        await foreach (var line in GetData(url, this.cancellationToken))
        {
        }
    }
}