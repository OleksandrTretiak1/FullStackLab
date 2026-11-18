using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace WpfClient
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new();
        private readonly string? apiKey;

        public MainWindow()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<MainWindow>()
                .Build();

            apiKey = config["OpenWeatherApiKey"];
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text;

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Введіть місто або перевірте наявність API ключа.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                ResultTextBlock.Text = json;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Помилка мережі – {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}