using ConsoleAppLibrary;
using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;

namespace ReadJsonPatchAsync;

public class Program
{
    [Patchable]
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int Temp { get; set; }
        public string Summary { get; set; }
    }

    public static async Task Main(string[] args)
    {
        LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeConsoleApp.TypeRepository.Instance.Extend(LaDeak.JsonMergePatch.Generated.SafeConsoleAppLibrary.TypeRepository.Instance);
        await ReadAsJsonMergePatchAsync();
    }

    public static async Task ReadAsJsonMergePatchAsync()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://localhost:5001/Sample/Weather", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadJsonPatchAsync<WeatherForecast>().ConfigureAwait(false);
        var target = new WeatherForecast() { Date = DateTime.UtcNow, Summary = "Sample weather forecast", Temp = 24 };
        var result = responseData.ApplyPatch(target);
        Console.WriteLine($"Patched: Date={result.Date}, Summary={result.Summary}, Temp={result.Temp}");

        var client = new Client();
        await client.ReadAsJsonMergePatchAsync();
    }
}
