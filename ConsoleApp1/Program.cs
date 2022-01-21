namespace ConsoleApp1;

using Z016.YahooFinanceApi;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var client = new YahooHistoricalClient("msft");
        Console.WriteLine($"IsSuccessful = {client.IsSuccessful}");
        await Task.Delay(1);
        ////IAsyncEnumerator<string?> enumerator = client.GetAsyncEnumerator();
       
        await foreach (var item in client)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine("Done");
    }
}