// <copyright file="YahooClientPricesTest.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi.Test
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Yahoo client prices Test class.
    /// </summary>
    public class YahooClientPricesTest
    {
        private readonly ITestOutputHelper loggerHelper;
        private readonly YahooClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooClientPricesTest"/> class.
        /// </summary>
        /// <param name="loggerHelper">ITestOutputHelper.</param>
        public YahooClientPricesTest(ITestOutputHelper loggerHelper)
        {
            this.loggerHelper = loggerHelper;

            var logger = this.loggerHelper.BuildLoggerFor<YahooClient>();
            this.client = new YahooClient(logger);
        }

        /// <summary>
        /// Get prices test.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetPricesTest()
        {
            DateOnly startDate = new(2022, 1, 3);
            var response = await this.client.GetPricesAsync("qqq", startDate, startDate.AddDays(5));

            Assert.NotNull(response);
            Assert.True(response.IsSuccessful);
            Assert.Equal(5, response.Prices.Count());
        }
    }
}