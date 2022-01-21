// <copyright file="YahooHistoricalClient.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi;

using Microsoft.Extensions.Logging;

/// <summary>
/// Yahoo historical stock data client class.
/// </summary>
public partial class YahooHistoricalClient : YahooClientBase, IAsyncEnumerable<string?> /*, IEnumerable<string>*/
{
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
        CancellationToken cancellationToken = default)
    {
        this.logger = logger;
        this.cancellationToken = cancellationToken;

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

        var url = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={Crumb}";
        Task.Run(() => this.SetResponse(url, cancellationToken), cancellationToken);

        //// TODO: async constructor pattern
        //// https://stackoverflow.com/questions/8145479/can-constructors-be-async/31471915#31471915
    }

    /// <inheritdoc/>
    public async IAsyncEnumerator<string?> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var enumerator = this.GetData(this.cancellationToken);

        await foreach (var line in enumerator)
        {
            yield return line;
        }

        ////List<string> result = new();
        ////result.Add("one");
        ////result.Add("two");

        ////foreach (var line in result)
        ////{
        ////    await Task.Delay(1, cancellationToken);
        ////    yield return line;
        ////}
    }

    /////// <inheritdoc/>
    ////public IEnumerator<string> GetEnumerator()
    ////{
    ////    List<string> result = new();
    ////    foreach (var line in result)
    ////    {
    ////        yield return line;
    ////    }
    ////}

    /////// <inheritdoc/>
    ////IEnumerator IEnumerable.GetEnumerator()
    ////{
    ////    List<string> result = new();
    ////    foreach (var line in result)
    ////    {
    ////        yield return line;
    ////    }
    ////}

    //// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/async-streams
    ////    IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator();
    ////    try
    ////    {
    ////        while (await enumerator.MoveNextAsync())
    ////        {
    ////            Use(enumerator.Current);
    ////        }
    ////    }
    ////    finally { await enumerator.DisposeAsync(); }
}