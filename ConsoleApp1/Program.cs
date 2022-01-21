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
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Start");

        var client = new YahooClient(loggerFactory.CreateLogger<YahooClient>());
        var response = await client.GetPrices("msft");
        logger.LogInformation("IsSuccessful = {IsSuccessful}  StatusCode = {StatusCode}", response.IsSuccessful, response.StatusCode);

        await foreach (var item in response)
        {
            Console.WriteLine(item);
        }

        logger.LogInformation("End");
        Console.WriteLine("Done");
    }
}