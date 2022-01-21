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
    private HttpResponseMessage? response;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooPricesResult"/> class.
    /// Successful constructor.
    /// </summary>
    /// <param name="response">HttpResponseMessage.</param>
    internal YahooPricesResult(HttpResponseMessage response)
    {
        this.response = response;
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
    /// Gets the contains value of status codes defined for HTTP.
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <inheritdoc/>
    public async IAsyncEnumerator<YahooPrice> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (this.response == null)
        {
            throw new ArgumentNullException("{response}  Enumerator can only be read once.", nameof(this.response));
        }

        using var responseStream = await this.response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            yield return new YahooPrice(new DateOnly(2021, 1, 1), 1, 2, 3, 4, 5, 6);
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