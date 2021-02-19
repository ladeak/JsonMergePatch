using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        private readonly ILogger<SampleController> _logger;

        public SampleController(ILogger<SampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
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
    }
}
