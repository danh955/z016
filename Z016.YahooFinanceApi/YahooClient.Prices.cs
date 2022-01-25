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
    public async Task<YahooPricesResult> GetPricesAsync(
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
            return new YahooPricesResult(false, response.StatusCode, prices);
        }

        var result = this.GetPricesParserAsync(response, cancellationToken);

        if (result != null)
        {
            await foreach (var item in result)
            {
                prices.Add(
                    new YahooPrice(
                        Date: item.Date,
                        Open: item.Open,
                        High: item.High,
                        Low: item.Low,
                        Close: item.Close,
                        AdjClose: item.AdjClose,
                        Volume: item.Volume));
            }
        }

        return new YahooPricesResult(true, response.StatusCode, prices);
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
    public async Task<YahooPricesParserResult> GetPricesParserAsync(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        var response = await this.GetPricesResponseAsync(symbol, firstDate, lastDate, interval, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new YahooPricesParserResult(false, response.StatusCode, AsyncEnumerable.Empty<YahooPriceParser>());
        }

        var result = this.GetPricesParserAsync(response, cancellationToken);
        return new YahooPricesParserResult(true, response.StatusCode, result);
    }

    private async IAsyncEnumerable<YahooPriceParser> GetPricesParserAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(responseStream);

        // Skip header.
        var line = await reader.ReadLineAsync();
        if (line != null)
        {
            var parser = new YahooPriceParser(this.logger);
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                line = await reader.ReadLineAsync();
                if (line != null)
                {
                    if (parser.SplitLine(line))
                    {
                        yield return parser;
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
    /// <param name="IsSuccessful">Gets a value indicating whether successful.</param>
    /// <param name="StatusCode">Gets the value of status codes from the HTTP request.</param>
    /// <param name="Prices">List of prices.</param>
    public record YahooPricesResult(bool IsSuccessful, HttpStatusCode StatusCode, IEnumerable<YahooPrice> Prices);

    /// <summary>
    /// Yahoo prices parser result.
    /// </summary>
    /// <param name="IsSuccessful">Gets a value indicating whether successful.</param>
    /// <param name="StatusCode">Gets the value of status codes from the HTTP request.</param>
    /// <param name="Prices">List of prices.</param>
    public record YahooPricesParserResult(bool IsSuccessful, HttpStatusCode StatusCode, IAsyncEnumerable<YahooPriceParser> Prices);

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