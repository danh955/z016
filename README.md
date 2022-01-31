# Get stock data from the unofficial Yahoo web API

This will access the unofficial Yahoo web API.
There API may not be available in the future.

## Using Hilres.YahooFinance

There are two function that will retrieve the historical stock prices.

- GetPricesAsync
- GetPricesParserAsync

### YahooClient class

This YahooClient class should be created only once when the application is started.
The logger is optional.

```csharp
public YahooClient(ILogger? logger = null)
```
Example using logging in a console appliction

```csharp
using Hilres.YahooFinanceClient;

private static class Program
{
    private static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Start");

        var client = new YahooClient(loggerFactory.CreateLogger<YahooClient>());

        /* more code */

        logger.LogInformation("End");
    }
}
````

### GetPricesAsync function

This will retrieve a IEnumerable&lt;YahooPrice&gt; price list.

```csharp
public async Task<YahooPricesResult> GetPricesAsync(
    string symbol,
    DateOnly? firstDate = null,
    DateOnly? lastDate = null,
    YahooInterval interval = YahooInterval.Daily,
    CancellationToken cancellationToken = default)
```
Example:

```csharp
var client = new YahooClient();
var response = await client.GetPricesAsync("msft", new(2022, 1, 3), new(2022, 1, 8));

if (response.IsSuccessful)
{
    foreach (var item in response.Prices)
    {
        Console.WriteLine(item);
    }
}
```

### GetPricesParserAsync function

This will retrive a IAsyncEnumerable&lt;YahooPriceParser&gt; price list.
YahooPriceParser is used to convert the data and is not an object to use.
The IAsyncEnumerable can only be used once and then the connection is closed.

```csharp
public async Task<YahooPricesParserResult> GetPricesParserAsync(
    string symbol,
    DateOnly? firstDate = null,
    DateOnly? lastDate = null,
    YahooInterval interval = YahooInterval.Daily,
    CancellationToken cancellationToken = default)
```
Example:

```csharp
var parser = await client.GetPricesParserAsync("msft", new(2022, 1, 3), new(2022, 1, 8));
if (parser.IsSuccessful)
{
    var prices = await parser.Prices
            .Select(p => new MyPrice(p.Date, p.AdjClose, p.Volume))
            .ToDictionaryAsync(p => p.Date)
            .ConfigureAwait(false);

    Console.WriteLine(prices[new(2022, 1, 4)]);
    Console.WriteLine(prices[new(2022, 1, 6)]);
}

public record MyPrice(DateOnly Date, double? AdjClose, long? Volume);
```
The cool thing about GetPricesParserAsync is that it minimizes 
creating extra objects. In this case, the values are going directly
into the dictionary.  GetPricesAsync is a warper that calls
GetPricesParserAsync
