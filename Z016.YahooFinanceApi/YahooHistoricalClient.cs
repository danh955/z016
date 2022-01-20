// <copyright file="YahooHistoricalClient.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using Microsoft.Extensions.Logging;

/// <summary>
/// Yahoo historical stock data client class.
/// </summary>
public partial class YahooHistoricalClient : YahooClientBase
{
    private readonly string url;
    private readonly ILogger<YahooHistoricalClient>? logger;
    private readonly CancellationToken cancellationToken;

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

        this.logger?.LogInformation("{symbol}", symbol);

        string period1 = firstDate.HasValue ? firstDate.Value.ToUnixTimestamp() : Constant.EpochString;
        string period2 = lastDate.HasValue ? lastDate.Value.ToUnixTimestamp() : DateTime.Today.ToUnixTimestamp();
        string intervalString = ToIntervalString(interval);

        this.url = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={Crumb}";
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

        await this.CheckCrumb(this.cancellationToken);

        await foreach (var line in GetData(this.url, this.cancellationToken))
        {
        }
    }
}