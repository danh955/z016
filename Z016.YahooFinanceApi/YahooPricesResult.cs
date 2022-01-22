// <copyright file="YahooPricesResult.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Net;
using System.Threading;

/// <summary>
/// Yahoo prices result class.
/// </summary>
public class YahooPricesResult : IAsyncEnumerable<YahooPrice>
{
    private readonly CancellationToken cancellationToken;
    private HttpResponseMessage? response;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooPricesResult"/> class.
    /// Successful constructor.
    /// </summary>
    /// <param name="response">HttpResponseMessage.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    internal YahooPricesResult(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        this.response = response;
        this.cancellationToken = cancellationToken;
        this.IsSuccessful = true;
        this.StatusCode = response.StatusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooPricesResult"/> class.
    /// Unsuccessful constructor.
    /// </summary>
    /// <param name="statusCode">HttpStatusCode.</param>
    internal YahooPricesResult(HttpStatusCode statusCode)
    {
        this.response = null;
        this.IsSuccessful = false;
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// Gets a value indicating whether successful.
    /// </summary>
    public bool IsSuccessful { get; private set; }

    /// <summary>
    /// Gets the value of status codes from the HTTP request.
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <inheritdoc/>
    public async IAsyncEnumerator<YahooPrice> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (this.response == null)
        {
            throw new ArgumentNullException("{response}  Enumerator can only be read once.", nameof(this.response));
        }

        if (cancellationToken == default && this.cancellationToken != default)
        {
            cancellationToken = this.cancellationToken;
        }

        using var responseStream = await this.response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
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
                                date.Value,
                                column[1].GetDouble(),
                                column[2].GetDouble(),
                                column[3].GetDouble(),
                                column[4].GetDouble(),
                                column[5].GetDouble(),
                                column[6].GetLong());
                        }
                    }
                }
            } // While loop
        }

        this.response = null;
    }
}

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