using System;
using System.Net.Http;
using System.Threading.Tasks;
using LaDeak.JsonMergePatch.Http;

namespace ConsoleAppLibrary
{
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
            var original = new DeviceData() { Watts = 5, Name = "phone" };
            var result = responseData.ApplyPatch(original);
            Console.WriteLine($"Patched: Name={result.Name}, Watts={result.Watts}");
        }
    }
}
