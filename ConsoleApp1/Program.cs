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

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var client = new YahooClient(loggerFactory.CreateLogger<YahooClient>());
        var response = await client.GetPrices("msft", today.AddDays(-7), today);
        logger.LogInformation("IsSuccessful = {IsSuccessful}  StatusCode = {StatusCode}", response.IsSuccessful, response.StatusCode);

        if (response.IsSuccessful)
        {
            await foreach (var item in response)
            {
                Console.WriteLine(item);
            }
        }

        logger.LogInformation("End");
        Console.WriteLine("Done");
    }
}