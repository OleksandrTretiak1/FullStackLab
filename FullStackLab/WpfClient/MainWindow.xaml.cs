using System;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;

namespace WpfClient
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new();
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly string? apiKey;
        private const string DbConnectionString = "Data Source=weather_app.db";

        public MainWindow()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<MainWindow>()
                .Build();

            apiKey = config["OpenWeatherApiKey"];
            InitializeDatabase();

            DisplayDataFromDatabase();
        }

        private static void InitializeDatabase()
        {
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cities (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE NOT NULL
                );
                CREATE TABLE IF NOT EXISTS WeatherConditions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CityId INTEGER,
                    Temperature REAL,
                    Description TEXT,
                    RecordDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(CityId) REFERENCES Cities(Id)
                );";
            cmd.ExecuteNonQuery();
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

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("Місто не знайдено. Перевірте правильність написання.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Помилка сервера. Код статусу {response.StatusCode}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                var weatherData = JsonSerializer.Deserialize<WeatherResponse>(json, jsonOptions);

                if (weatherData != null)
                {
                    SaveToDatabase(weatherData);
                    DisplayDataFromDatabase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Виникла помилка {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SaveToDatabase(WeatherResponse data)
        {
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();

            var insertCityCmd = conn.CreateCommand();
            insertCityCmd.CommandText = "INSERT OR IGNORE INTO Cities (Name) VALUES ($name)";
            insertCityCmd.Parameters.AddWithValue("$name", data.Name);
            insertCityCmd.ExecuteNonQuery();

            var getCityIdCmd = conn.CreateCommand();
            getCityIdCmd.CommandText = "SELECT Id FROM Cities WHERE Name = $name";
            getCityIdCmd.Parameters.AddWithValue("$name", data.Name);

            object? scalarResult = getCityIdCmd.ExecuteScalar();
            long cityId = scalarResult != null ? (long)scalarResult : 0;

            string? desc = data.Weather != null && data.Weather.Count > 0 ? data.Weather[0].Description : "немає даних";
            var insertWeatherCmd = conn.CreateCommand();
            insertWeatherCmd.CommandText = "INSERT INTO WeatherConditions (CityId, Temperature, Description) VALUES ($cityId, $temp, $desc)";
            insertWeatherCmd.Parameters.AddWithValue("$cityId", cityId);
            insertWeatherCmd.Parameters.AddWithValue("$temp", data.Main?.Temp ?? 0);
            insertWeatherCmd.Parameters.AddWithValue("$desc", desc);
            insertWeatherCmd.ExecuteNonQuery();
        }

        private void DisplayDataFromDatabase()
        {
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Cities.Name, WeatherConditions.Temperature, WeatherConditions.Description, WeatherConditions.RecordDate
                FROM Cities
                JOIN WeatherConditions ON Cities.Id = WeatherConditions.CityId
                ORDER BY WeatherConditions.RecordDate DESC";

            using var reader = cmd.ExecuteReader();
            string result = "Історія запитів\n\n";
            while (reader.Read())
            {
                string name = reader.GetString(0);
                double temp = reader.GetDouble(1);
                string desc = reader.GetString(2);
                string date = reader.GetDateTime(3).ToLocalTime().ToString("g");
                result += $"{date}, Місто {name}, Температура {temp}°C, Стан {desc}\n";
            }

            ResultTextBlock.Text = result;
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = "DELETE FROM WeatherConditions; DELETE FROM Cities;";
            cmd.ExecuteNonQuery();

            DisplayDataFromDatabase();
        }
    }
}