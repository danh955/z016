// <copyright file="YahooClient.Prices.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using Microsoft.Extensions.Logging;

/// <summary>
/// Historical data retrieval for Yahoo client class.
/// </summary>
public partial class YahooClient
{
    /// <summary>
    /// Get historical stock prices.
    /// </summary>
    /// <param name="symbol">Symbol of prices to get.</param>
    /// <param name="firstDate">First date.</param>
    /// <param name="lastDate">Last date.</param>
    /// <param name="interval">Stock price interval.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task GetPrices(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("{symbol}", symbol);

        string period1 = firstDate.HasValue ? firstDate.Value.ToUnixTimestamp() : Constant.EpochString;
        string period2 = lastDate.HasValue ? lastDate.Value.ToUnixTimestamp() : DateTime.Today.ToUnixTimestamp();
        string intervalString = ToIntervalString(interval);

        await this.CheckCrumb(cancellationToken);
        var url = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={this.crumb}";

        var response = await this.apiHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
    }
}