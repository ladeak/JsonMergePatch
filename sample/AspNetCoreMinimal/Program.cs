using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllers().AddMvcOptions(options =>
{
    LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.TypeRepository.Instance;
    var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
    jsonOptions.SerializerOptions.AddContext<SampleJsonContext>();
    options.InputFormatters.Insert(0, new JsonMergePatchInputReader(jsonOptions));
});
builder.Services.AddHttpClient();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.WeatherForecastWrapped))]
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.CitiesDataWrapped))]
public partial class SampleJsonContext : JsonSerializerContext
{
}