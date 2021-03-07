using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreWebApi.Controllers
{
    public class CitiesData
    {
        public Dictionary<string, string> Cities { get; set; }
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        [JsonPropertyName("temp")]
        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }

    public class DeviceData
    {
        public double Watts { get; set; }
        public string Name { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IHttpClientFactory _clientFactory;

        public SampleController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet("Weather")]
        public WeatherForecast GetWeather()
        {
            var rng = new Random();
            return new WeatherForecast
            {
                Date = DateTime.Now.AddDays(1),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };
        }

        [HttpGet("DeviceData")]
        public DeviceData GetDeviceData()
        {
            return new DeviceData
            {
                Name = "test device1",
                Watts = 12
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
}
