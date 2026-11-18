using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WpfClient
{
    public class WeatherResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("main")]
        public MainData? Main { get; set; }

        [JsonPropertyName("weather")]
        public List<WeatherInfo>? Weather { get; set; }
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }
    }

    public class WeatherInfo
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}