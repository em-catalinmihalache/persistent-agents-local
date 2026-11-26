using System.ComponentModel;

namespace PersistentAgents.Local.Agents.Tools;

public static class WeatherOptionsTool
{
    [Description("Returns simulated weather information using optional forecast settings.")]
    public static string GetWeather(
        [Description("The city name.")]
        string city,

        [Description("The country name.")]
        string country,

        [Description("Optional weather forecast settings.")]
        WeatherOptions? options = null)
    {
        Console.WriteLine("[Tool Executed] GetWeather entered.");

        Console.WriteLine($"[Tool Arguments] City: {city}");
        Console.WriteLine($"[Tool Arguments] Country: {country}");
        Console.WriteLine($"[Tool Arguments] Options: {options}");

        var unit = options?.Unit ?? "Celsius";
        var days = options?.Days ?? 1;

        Console.WriteLine(
            $"[Tool Defaults] Unit: {unit}, Days: {days}");

        if (days < 1 || days > 7)
        {
            return "The number of forecast days must be between 1 and 7.";
        }

        var weather = city.ToLowerInvariant() switch
        {
            "bucharest" => (Condition: "Sunny", Celsius: 18),
            "rome" => (Condition: "Cloudy", Celsius: 21),
            "london" => (Condition: "Rainy", Celsius: 12),
            _ => (Condition: "Unknown", Celsius: 0)
        };

        if (weather.Condition == "Unknown")
        {
            return $"Weather information is unavailable for {city}, {country}.";
        }

        var temperature = unit.ToLowerInvariant() switch
        {
            "fahrenheit" => weather.Celsius * 9.0 / 5.0 + 32,
            "celsius" => weather.Celsius,
            _ => (double?)null
        };

        if (temperature is null)
        {
            return $"Unsupported temperature unit: {unit}.";
        }

        var symbol = unit.Equals(
            "fahrenheit",
            StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";

        return $"{weather.Condition}, {temperature:0.#}{symbol} in {city}, {country}. Forecast period: {days} day(s).";
    }
}