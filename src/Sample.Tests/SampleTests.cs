using System;
using Sample.Models;
using Xunit;

namespace Sample.Tests
{
    public class SampleTests
    {
        [Fact]
        public void SampleTest()
        {
            WeatherForecast forecast = new WeatherForecast() { Summary = "TestSummary", TemperatureC = 0 };
            Assert.Equal("TestSummary", forecast.Summary);
            Assert.Equal(32, forecast.TemperatureF);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        public void InlineDataExample(int number, bool expectedResult)
        {
            Console.WriteLine("InlineDataExample(int {0}, bool {1})", number, expectedResult);
            Assert.Equal(expectedResult, number < 3);
        }
    }
}
