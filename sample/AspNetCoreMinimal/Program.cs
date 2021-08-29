using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var mvcBuilder = builder.Services.AddControllers();
AddJsonMergePatch(mvcBuilder);
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AspNetCoreMinimal", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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


IMvcBuilder AddJsonMergePatch(IMvcBuilder builder)
{
    LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.TypeRepository.Instance;
    builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>(LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository);
    var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
    jsonOptions.SerializerOptions.AddContext<SampleJsonContext>();
    return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
}

[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.WeatherForecastWrapped))]
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.CitiesDataWrapped))]
[JsonSerializable(typeof(LaDeak.JsonMergePatch.Generated.SafeAspNetCoreMinimal.Entities.DeviceDataWrapped))]
public partial class SampleJsonContext : JsonSerializerContext
{
}