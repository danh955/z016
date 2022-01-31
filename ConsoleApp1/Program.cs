namespace ConsoleApp1;

using Microsoft.Extensions.Logging;
using Z016.YahooFinanceApi;

internal class Program
{
    private static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Z016", LogLevel.Trace)
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Start");

        var client = new YahooClient(loggerFactory.CreateLogger<YahooClient>());
        var response = await client.GetPricesAsync("msft", new(2022, 1, 3), new(2022, 1, 8));
        logger.LogInformation("IsSuccessful = {IsSuccessful}  StatusCode = {StatusCode}", response.IsSuccessful, response.StatusCode);

        if (response.IsSuccessful)
        {
            foreach (var item in response.Prices)
            {
                Console.WriteLine(item);
            }
        }

        var parser = await client.GetPricesParserAsync("msft", new(2022, 1, 3), new(2022, 1, 8));
        logger.LogInformation("IsSuccessful = {IsSuccessful}  StatusCode = {StatusCode}", response.IsSuccessful, response.StatusCode);

        if (parser.IsSuccessful)
        {
            var prices = await parser.Prices
                    .Select(p => new MyPrice(p.Date, p.AdjClose, p.Volume))
                    .ToDictionaryAsync(p => p.Date)
                    .ConfigureAwait(false);

            Console.WriteLine(prices[new(2022, 1, 4)]);
            Console.WriteLine(prices[new(2022, 1, 6)]);
        }

        logger.LogInformation("End");
        Console.WriteLine("Done");
    }

    public record MyPrice(DateOnly Date, double? AdjClose, long? Volume);
}