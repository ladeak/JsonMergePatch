using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch.Abstractions;

namespace AspNetCoreMinimal.Entities;

[Patchable]
public class CitiesData
{
    public Dictionary<string, string> Cities { get; set; }
}

[Patchable]
public class WeatherForecast
{
    public DateTime Date { get; set; }

    [JsonPropertyName("temp")]
    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; set; }

    public List<Tag> Tags { get; set; }
}

[Patchable]
public class Tag
{
    public int Id { get; set; }

    public string? Name { get; set; }
}
