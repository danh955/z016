// <copyright file="YahooClient.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Yahoo client class.
/// </summary>
public partial class YahooClient
{
    private readonly HttpClient apiHttpClient = new();
    private readonly HttpClient crumbHttpClient = new();
    private readonly ILogger? logger;

    private readonly Regex regexCrumb = new(
                                "\"CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                                RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private string crumb = string.Empty;
    private DateTime nextCrumbTime = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooClient"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    public YahooClient(ILogger? logger = null)
    {
        this.logger = logger;

        const string UserAgent = "user-agent";
        const string UserAgentText = "Mozilla/5.0 (X11; U; Linux i686) Gecko/20071127 Firefox/2.0.0.11";

        this.apiHttpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
        this.crumbHttpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
    }

    /// <summary>
    /// Gets or sets the interval time between resetting the cookie crumb in minutes.
    /// </summary>
    public int CrumbResetInterval { get; set; } = 5;

    private static string? GetCookie(HttpResponseMessage responseMessage, string cookieName)
    {
        var keyValue = responseMessage.Headers.FirstOrDefault(h => h.Key == cookieName);
        return keyValue.Value?.FirstOrDefault();
    }

    /// <summary>
    /// Convert the interval into a string.
    /// </summary>
    /// <param name="interval">StockInterval.</param>
    /// <returns>string.</returns>
    private static string ToIntervalString(YahooInterval? interval) => interval switch
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
    private async Task CheckCrumb(CancellationToken cancellationToken)
    {
        if (this.nextCrumbTime < DateTime.Now || this.crumb == null)
        {
            this.nextCrumbTime = DateTime.Now.AddMinutes(this.CrumbResetInterval);
            await this.RefreshCookieAndCrumbAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Get the cookie and crumb value.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>True if successful.</returns>
    private async Task RefreshCookieAndCrumbAsync(CancellationToken cancellationToken)
    {
        const int maxTryCount = 5;
        int tryCount = 0;

        var (cookie, newCrumb) = await this.TryGetCookieAndCrumbAsync().ConfigureAwait(false);

        while (newCrumb == null && !cancellationToken.IsCancellationRequested && tryCount < maxTryCount)
        {
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            (cookie, newCrumb) = await this.TryGetCookieAndCrumbAsync().ConfigureAwait(false);
            tryCount++;
        }

        this.logger?.LogTrace("Got cookie. tryCount = {TryCount}, Crumb = {Crumb}, Cookie = {Cookie}", tryCount, this.crumb, cookie);
        this.apiHttpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Cookie.ToString(), cookie);
        this.crumb = newCrumb ?? string.Empty;
    }

    /// <summary>
    /// Try to get the cookie and crumb value.
    /// </summary>
    /// <returns>(cookie, crumb) if successful.  Otherwise (null, null) or (cookie, null).</returns>
    private async Task<(string? Cookie, string? Crumb)> TryGetCookieAndCrumbAsync()
    {
        const string uri = "https://finance.yahoo.com/quote/%5EGSPC";

        var response = await this.crumbHttpClient.GetAsync(uri).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var cookie = GetCookie(response, "Set-Cookie");
            if (string.IsNullOrEmpty(cookie))
            {
                this.logger?.LogWarning("{Function}  Cookie not found", nameof(this.TryGetCookieAndCrumbAsync));
                return (null, null);
            }

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var matches = this.regexCrumb.Matches(html);
            if (matches.Count != 1)
            {
                this.logger?.LogWarning("{Function}  Crumb not found", nameof(this.TryGetCookieAndCrumbAsync));
                return (cookie, null);
            }

            var crumb = matches[0].Groups["crumb"]?.Value?.Replace("\\u002F", "/");
            if (string.IsNullOrWhiteSpace(crumb))
            {
                this.logger?.LogWarning("{Function}  Crumb is empty", nameof(this.TryGetCookieAndCrumbAsync));
                return (cookie, null);
            }

            return (cookie, crumb);
        }

        return (null, null);
    }
}