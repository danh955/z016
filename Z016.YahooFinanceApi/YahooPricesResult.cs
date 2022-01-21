// <copyright file="YahooPricesResult.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Net;

/// <summary>
/// Yahoo prices result class.
/// </summary>
public class YahooPricesResult
{
    private readonly HttpResponseMessage? response;

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