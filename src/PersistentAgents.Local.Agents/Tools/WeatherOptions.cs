using System.ComponentModel;

namespace PersistentAgents.Local.Agents.Tools;

[Description("Optional weather forecast settings.")]
public sealed record WeatherOptions(
    [property: Description("The temperature unit: Celsius or Fahrenheit. Optional.")]
    string? Unit,

    [property: Description("The number of forecast days. Optional.")]
    int? Days);
