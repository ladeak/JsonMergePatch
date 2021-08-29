using System.Text.Json;
using AspNetCoreMinimal.Entities;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreMinimal.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public SampleController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet("Weather")]
    public WeatherForecast GetWeather()
    {
        return new WeatherForecast
        {
            Date = DateTime.Now.AddDays(1),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = "Sample weather forecast"
        };
    }

    [HttpPatch("PatchWeather")]
    public WeatherForecast PatchForecast(Patch<WeatherForecast> input)
    {
        var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
        var result = input.ApplyPatch(target);
        return result;
    }

    [HttpPatch("PatchCities")]
    public CitiesData PatchCities(Patch<CitiesData> input)
    {
        var target = new CitiesData() { Cities = new Dictionary<string, string>() { { "Frankfurt", "Germany" }, { "New York", "US" }, { "London", "UK" } } };
        var result = input.ApplyPatch(target);
        return result;
    }

    [HttpGet("ReadJsonPatchAsync")]
    public async Task<WeatherForecast> GetReadJsonPatchAsync()
    {
        var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
        var httpClient = _clientFactory.CreateClient();
        var response = await httpClient.GetAsync("https://localhost:5001/Sample/Weather", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadJsonPatchAsync<WeatherForecast>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }).ConfigureAwait(false);
        var result = responseData.ApplyPatch(target);
        return result;
    }
}
