using LaDeak.JsonMergePatch.Abstractions;
using LaDeak.JsonMergePatch.Http;

namespace ConsoleAppLibrary;

[Patchable]
public class DeviceData
{
    public double Watts { get; set; }
    public string Name { get; set; }
}

public class Client
{
    public async Task ReadAsJsonMergePatchAsync()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://localhost:5001/Sample/DeviceData", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadJsonPatchAsync<DeviceData>().ConfigureAwait(false);
        var target = new DeviceData() { Watts = 5, Name = "phone" };
        var result = responseData.ApplyPatch(target);
        Console.WriteLine($"Patched: Name={result.Name}, Watts={result.Watts}");
    }
}
