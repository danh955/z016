// <copyright file="YahooClientBase.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

/// <summary>
/// Yahoo client base class.
/// </summary>
public abstract class YahooClientBase
{
    private static readonly HttpClient ApiHttpClient;
    private static readonly HttpClient CrumbHttpClient;
    private static DateTime nextCrumbTime = DateTime.MinValue;

    private readonly Regex regexCrumb = new(
                            "\"CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                            RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Initializes static members of the <see cref="YahooClientBase"/> class.
    /// </summary>
    static YahooClientBase()
    {
        Crumb = string.Empty;

        const string UserAgent = "user-agent";
        const string UserAgentText = "Mozilla/5.0 (X11; U; Linux i686) Gecko/20071127 Firefox/2.0.0.11";

        ApiHttpClient = new(); // With Set-Cookie.
        ApiHttpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
        CrumbHttpClient = new();  // No Set-Cookie.
        CrumbHttpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
    }

    /// <summary>
    /// Gets or sets the interval time between resetting the cookie crumb in minutes.
    /// </summary>
    public int CrumbResetInterval { get; set; } = 5;

    /// <summary>
    /// Gets the Yahoo crumb to use.
    /// </summary>
    protected static string Crumb { get; private set; }

    /// <summary>
    /// Get data stream.
    /// </summary>
    /// <param name="url">URL to get the data.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>List of strings.</returns>
    protected static async IAsyncEnumerable<string?> GetData(string url, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = await ApiHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(responseStream);

            while (!reader.EndOfStream)
            {
                yield return await reader.ReadLineAsync();
            }
        }
    }

    /// <summary>
    /// Convert the interval into a string.
    /// </summary>
    /// <param name="interval">StockInterval.</param>
    /// <returns>string.</returns>
    protected static string ToIntervalString(YahooInterval? interval) => interval switch
    {
        null => "1d",
        YahooInterval.Daily => "1d",
        YahooInterval.Weekly => "1wk",
        YahooInterval.Monthly => "1mo",
        YahooInterval.Quorterly => "3mo",
        _ => throw new NotImplementedException(interval.ToString()),
    };

    /// <summary>
    /// Check if we need a new crumb.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Task.</returns>
    protected async Task CheckCrumb(CancellationToken cancellationToken)
    {
        if (nextCrumbTime < DateTime.Now || Crumb == null)
        {
            nextCrumbTime = DateTime.Now.AddMinutes(this.CrumbResetInterval);
            await this.RefreshCookieAndCrumbAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Get the cookie and crumb value.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>True if successful.</returns>
    protected async Task RefreshCookieAndCrumbAsync(CancellationToken cancellationToken)
    {
        const int maxTryCount = 5;
        int tryCount = 0;

        var (cookie, newCrumb) = await this.TryGetCookieAndCrumbAsync();

        while (newCrumb == null && !cancellationToken.IsCancellationRequested && tryCount < maxTryCount)
        {
            await Task.Delay(1000, cancellationToken);
            (cookie, newCrumb) = await this.TryGetCookieAndCrumbAsync();
            tryCount++;
        }

        ////this.logger.LogInformation("Got cookie. tryCount = {TryCount}, Crumb = {Crumb}, Cookie = {Cookie}", tryCount, crumb, cookie);
        ApiHttpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Cookie.ToString(), cookie);
        Crumb = newCrumb ?? string.Empty;
    }

    private static string? GetCookie(HttpResponseMessage responseMessage, string cookieName)
    {
        var keyValue = responseMessage.Headers.SingleOrDefault(h => h.Key == cookieName);
        return keyValue.Value?.SingleOrDefault();
    }

    /// <summary>
    /// Try to get the cookie and crumb value.
    /// </summary>
    /// <returns>(cookie, crumb) if successful.  Otherwise (null, null) or (cookie, null).</returns>
    private async Task<(string? Cookie, string? Crumb)> TryGetCookieAndCrumbAsync()
    {
        const string uri = "https://finance.yahoo.com/quote/%5EGSPC";

        var response = await CrumbHttpClient.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var cookie = GetCookie(response, "Set-Cookie");
            if (string.IsNullOrEmpty(cookie))
            {
                ////this.logger.LogWarning("{Function}  Cookie not found", nameof(this.TryGetCookieAndCrumbAsync));
                return (null, null);
            }

            var html = await response.Content.ReadAsStringAsync();
            var matches = this.regexCrumb.Matches(html);
            if (matches.Count != 1)
            {
                ////this.logger.LogWarning("{Function}  Crumb not found", nameof(this.TryGetCookieAndCrumbAsync));
                return (cookie, null);
            }

            var crumb = matches[0].Groups["crumb"]?.Value?.Replace("\\u002F", "/");
            if (string.IsNullOrWhiteSpace(crumb))
            {
                ////this.logger.LogWarning("{Function}  Crumb is empty", nameof(this.TryGetCookieAndCrumbAsync));
                return (cookie, null);
            }

            return (cookie, crumb);
        }

        return (null, null);
    }
}