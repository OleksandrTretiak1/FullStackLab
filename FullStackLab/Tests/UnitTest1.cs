using Xunit;
using WpfClient;
using System;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void BuildApiUrl_ReturnsCorrectFormattedString()
        {
            string city = "Kyiv";
            string key = "testkey123";

            string result = WeatherService.BuildApiUrl(city, key);

            Assert.Equal("https://api.openweathermap.org/data/2.5/weather?q=Kyiv&appid=testkey123&units=metric", result);
        }

        [Fact]
        public void BuildApiUrl_EmptyCity_ThrowsException()
        {

            Assert.Throws<ArgumentException>(() => WeatherService.BuildApiUrl("", "testkey"));
        }
    }
}