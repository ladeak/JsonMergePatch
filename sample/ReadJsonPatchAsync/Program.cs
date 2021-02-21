using System;
using System.Net.Http;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.Http;

namespace ReadJsonPatchAsync
{
    public class Program
    {
        public class WeatherForecast
        {
            public DateTime Date { get; set; }
            public int Temp { get; set; }
            public string Summary { get; set; }
        }

        public static async Task Main(string[] args)
        {
            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.TypeRepositoryContainer.Instance.Repository;
            await ReadAsJsonMergePatchAsync();
        }

        public static async Task ReadAsJsonMergePatchAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:5001/Sample/Weather", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var responseData = await response.Content.ReadJsonPatchAsync<WeatherForecast>().ConfigureAwait(false);
            var original = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", Temp = 24 };
            var result = responseData.ApplyPatch(original);
            Console.WriteLine($"Patched: Date={result.Date}, Summary={result.Summary}, Temp={result.Temp}");
        }
    }
}
