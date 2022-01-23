﻿// <copyright file="YahooClient.Prices.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Net;
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
    public async Task<YahooPricesResult> GetPricesAsync(
        string symbol,
        DateOnly? firstDate = null,
        DateOnly? lastDate = null,
        YahooInterval interval = YahooInterval.Daily,
        CancellationToken cancellationToken = default)
    {
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
        List<YahooPrice> prices = new();

        this.logger?.LogInformation("{symbol}\n {url}", symbol, url);
        await this.CheckCrumb(cancellationToken);
        var response = await this.apiHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (response == null)
        {
            throw new Exception("HttpClient.GetAsync return null.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return new YahooPricesResult(false, response.StatusCode, prices);
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(responseStream);

        // Skip header.
        var line = await reader.ReadLineAsync();
        if (line == null)
        {
            return new YahooPricesResult(true, response.StatusCode, prices);
        }

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
                        prices.Add(new YahooPrice(
                            Date: date.Value,
                            Open: column[1].GetDouble(),
                            High: column[2].GetDouble(),
                            Low: column[3].GetDouble(),
                            Close: column[4].GetDouble(),
                            AdjClose: column[5].GetDouble(),
                            Volume: column[6].GetLong()));
                    }
                }
            }
        } // While loop

        return new YahooPricesResult(true, response.StatusCode, prices);
    }

    /// <summary>
    /// Yahoo prices result.
    /// </summary>
    /// <param name="IsSuccessful">Gets a value indicating whether successful.</param>
    /// <param name="StatusCode">Gets the value of status codes from the HTTP request.</param>
    /// <param name="Prices">List of prices.</param>
    public record YahooPricesResult(bool IsSuccessful, HttpStatusCode StatusCode, IEnumerable<YahooPrice> Prices);

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