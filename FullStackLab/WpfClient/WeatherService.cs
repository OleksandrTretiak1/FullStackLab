namespace WpfClient
{
    public class WeatherService
    {
        public string BuildApiUrl(string city, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(apiKey))
            {
                throw new System.ArgumentException("Місто або ключ не можуть бути порожніми");
            }
            return $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
        }
    }
}