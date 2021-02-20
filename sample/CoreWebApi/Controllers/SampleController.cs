using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch;
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

    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITypeRepository _typeRepository;

        public SampleController(IHttpClientFactory clientFactory, ITypeRepository typeRepository)
        {
            _clientFactory = clientFactory;
            _typeRepository = typeRepository;
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

        [HttpPatch("PatchWeather")]
        public WeatherForecast PatchForecast(Patch<WeatherForecast> input)
        {
            var original = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
            var result = input.ApplyPatch(original);
            return result;
        }

        [HttpPatch("PatchCities")]
        public CitiesData PatchCities(Patch<CitiesData> input)
        {
            var original = new CitiesData() { Cities = new Dictionary<string, string>() { { "Frankfurt", "Germany" }, { "New York", "US" }, { "London", "UK" } } };
            var result = input.ApplyPatch(original);
            return result;
        }

        [HttpGet("ReadJsonPatchAsync")]
        public async Task<WeatherForecast> GetReadJsonPatchAsync()
        {
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.GetAsync("https://localhost:5001/Sample/Weather", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var responseData = await response.Content.ReadJsonPatchAsync<WeatherForecast>(_typeRepository, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }).ConfigureAwait(false);
            var original = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", TemperatureC = 24 };
            var result = responseData.ApplyPatch(original);
            return result;
        }
    }
}
