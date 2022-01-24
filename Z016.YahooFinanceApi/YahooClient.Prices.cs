// <copyright file="YahooClient.Prices.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
    /// <returns>IEnumerable list of YahooPrice.</returns>
    public async Task<YahooPricesResult<IEnumerable<YahooPrice>>> GetPricesAsync(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        List<YahooPrice> prices = new();

        var response = await this.GetPricesResponseAsync(symbol, firstDate, lastDate, interval, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new YahooPricesResult<IEnumerable<YahooPrice>>(false, response.StatusCode, prices);
        }

        var result = this.GetPricesEnumerableAsync(response, cancellationToken);

        if (result != null)
        {
            await foreach (var item in result)
            {
                prices.Add(item);
            }
        }

        return new YahooPricesResult<IEnumerable<YahooPrice>>(true, response.StatusCode, prices);
    }

    /// <summary>
    /// Get historical stock prices.
    /// </summary>
    /// <param name="symbol">Symbol of prices to get.</param>
    /// <param name="firstDate">First date.</param>
    /// <param name="lastDate">Last date.</param>
    /// <param name="interval">Stock price interval.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>IAsyncEnumerable of YahooPrice.</returns>
    public async Task<YahooPricesResult<IAsyncEnumerable<YahooPrice>>> GetPricesEnumerableAsync(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        var response = await this.GetPricesResponseAsync(symbol, firstDate, lastDate, interval, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new YahooPricesResult<IAsyncEnumerable<YahooPrice>>(false, response.StatusCode, AsyncEnumerable.Empty<YahooPrice>());
        }

        var result = this.GetPricesEnumerableAsync(response, cancellationToken);
        return new YahooPricesResult<IAsyncEnumerable<YahooPrice>>(true, response.StatusCode, result);
    }

    private async IAsyncEnumerable<YahooPrice> GetPricesEnumerableAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(responseStream);

        // Skip header.
        var line = await reader.ReadLineAsync();
        if (line != null)
        {
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                line = await reader.ReadLineAsync();
                if (line != null)
                {
                    var column = line.Split(',');
                    if (column.Length == 7)
                    {
                        var date = column[0].GetDateOnly();
                        if (date != null)
                        {
                            yield return new YahooPrice(
                                Date: date.Value,
                                Open: column[1].GetDouble(),
                                High: column[2].GetDouble(),
                                Low: column[3].GetDouble(),
                                Close: column[4].GetDouble(),
                                AdjClose: column[5].GetDouble(),
                                Volume: column[6].GetLong());
                        }
                        else
                        {
                            this.logger?.LogDebug("Yahoo has an invalid date.\n {data}", line);
                        }
                    }
                    else
                    {
                        this.logger?.LogDebug("Yahoo did not return 7 columns.\n {data}", line);
                    }
                }
            } // While loop
        }
    }

    private async Task<HttpResponseMessage> GetPricesResponseAsync(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        this.logger?.LogDebug("GetPrices for {symbol}, firstDate = {firstDate}, lastDate = {lastDate}, interval = {interval}", symbol, firstDate, lastDate, interval);

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

        string period1 = firstDate.HasValue ? firstDate.Value.ToUnixTimestamp() : Constant.EpochString;
        string period2 = lastDate.HasValue ? lastDate.Value.ToUnixTimestamp() : DateTime.Today.ToUnixTimestamp();
        string intervalString = ToIntervalString(interval);
        var url = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={this.crumb}";
        this.logger?.LogTrace("{URL}", url);

        await this.CheckCrumb(cancellationToken);
        var response = await this.apiHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (response == null)
        {
            throw new Exception("HttpClient.GetAsync return null.");
        }

        return response;
    }

    /// <summary>
    /// Yahoo prices result.
    /// </summary>
    /// <typeparam name="T">Price list type.</typeparam>
    /// <param name="IsSuccessful">Gets a value indicating whether successful.</param>
    /// <param name="StatusCode">Gets the value of status codes from the HTTP request.</param>
    /// <param name="Prices">List of prices.</param>
    public record YahooPricesResult<T>(bool IsSuccessful, HttpStatusCode StatusCode, T Prices);

    /// <summary>
    /// A stock price record from the client.
    /// </summary>
    /// <param name="Date">Date of price.</param>
    /// <param name="Open">Opening price of the stock.</param>
    /// <param name="High">The high price.</param>
    /// <param name="Low">The low price.</param>
    /// <param name="Close">Closing price.</param>
    /// <param name="AdjClose">The adjusted closing price.</param>
    /// <param name="Volume">The volume traded.</param>
    public record YahooPrice(DateOnly Date, double? Open, double? High, double? Low, double? Close, double? AdjClose, long? Volume);
}