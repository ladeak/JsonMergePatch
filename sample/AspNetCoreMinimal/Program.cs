using System.Text.Json.Serialization;
using LaDeak.JsonMergePatch.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllers().AddMvcOptions(options =>
{
    LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.TypeRepository.Instance;
    options.InputFormatters.Insert(0, new JsonMergePatchInputReader(new Microsoft.AspNetCore.Http.Json.JsonOptions()));
});
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AspNetCoreMinimal", Version = "v1" });
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCoreMinimal v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.WeatherForecastWrapped))]
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.CitiesDataWrapped))]
public partial class SampleJsonContext : JsonSerializerContext
{
}