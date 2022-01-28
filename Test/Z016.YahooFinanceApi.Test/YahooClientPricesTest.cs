// <copyright file="YahooClientPricesTest.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z016.YahooFinanceApi.Test
{
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Yahoo client prices Test class.
    /// </summary>
    public class YahooClientPricesTest
    {
        private readonly YahooClient client;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooClientPricesTest"/> class.
        /// </summary>
        /// <param name="loggerHelper">ITestOutputHelper.</param>
        public YahooClientPricesTest(ITestOutputHelper loggerHelper)
        {
            this.logger = loggerHelper.BuildLogger();
            this.client = new YahooClient(this.logger);
        }

        /// <summary>
        /// Get prices with symbol is empty exception test.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetPricesEmptySymbolExceptionTest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.client.GetPricesAsync(string.Empty)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get prices with start date grater then end date exception test.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetPricesStartGraterThenEndExceptionTest()
        {
            var startDate = new DateOnly(2022, 1, 3);
            await Assert.ThrowsAsync<ArgumentException>(() => this.client.GetPricesAsync("QQQ", startDate, startDate.AddDays(-1))).ConfigureAwait(false);
        }

        /// <summary>
        /// Get prices parser in a IAsyncEnumerable list test.
        /// </summary>
        /// <param name="symbol">Stock symbol.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="interval">Interval of the data.</param>
        /// <param name="count">Expected number of rows returned.</param>
        /// <returns>Task.</returns>
        [Theory]
        [InlineData("QQQ", "1/3/2022", "1/3/2022", YahooInterval.Daily, null)]
        [InlineData("QQQ", "1/3/2022", "1/4/2022", YahooInterval.Daily, 1)]
        [InlineData("QQQ", "1/3/2022", "1/7/2022", YahooInterval.Daily, 4)]
        [InlineData("QQQ", "1/3/2022", "1/8/2022", YahooInterval.Daily, 5)]
        [InlineData("QQQ", "1/3/2022", "1/9/2022", YahooInterval.Daily, 5)]
        [InlineData("QQQ", "1/3/2022", "1/10/2022", YahooInterval.Daily, 5)]
        [InlineData("QQQ", "1/3/2022", "1/11/2022", YahooInterval.Daily, 6)]
        public async Task GetPricesParserAsyncTest(string symbol, string firstDate, string lastDate, YahooInterval interval, int? count)
        {
            var response = await this.client.GetPricesParserAsync(symbol, DateOnly.Parse(firstDate), DateOnly.Parse(lastDate), interval).ConfigureAwait(false);
            this.logger.LogDebug("{response}", response);

            Assert.NotNull(response);

            if (count == null)
            {
                Assert.False(response.IsSuccessful);
                return;
            }

            Assert.True(response.IsSuccessful);
            var rowCount = await response.Prices.CountAsync().ConfigureAwait(false);
            Assert.Equal(count, rowCount);
        }

        /// <summary>
        /// Get prices test.
        /// </summary>
        /// <param name="symbol">Stock symbol.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="interval">Interval of the data.</param>
        /// <param name="count">Expected number of rows returned.</param>
        /// <returns>Task.</returns>
        [Theory]
        [InlineData("QQQ", "3/9/2021", "3/14/2021", YahooInterval.Daily, 4)]
        [InlineData("QQQ", "12/14/2020", "3/13/2021", YahooInterval.Weekly, 13)]
        [InlineData("QQQ", "10/1/2020", "3/31/2021", YahooInterval.Monthly, 6)]
        [InlineData("QQQ", "1/1/2020", "12/31/2020", YahooInterval.Quorterly, 4)]
        public async Task GetPricesTest(string symbol, string firstDate, string lastDate, YahooInterval interval, int count)
        {
            var response = await this.client.GetPricesAsync(symbol, DateOnly.Parse(firstDate), DateOnly.Parse(lastDate), interval).ConfigureAwait(false);
            this.logger.LogDebug("{response}", response);

            Assert.NotNull(response);
            Assert.True(response.IsSuccessful);
            Assert.Equal(count, response.Prices.Count());
        }
    }
}