using JsonMergePatch.Shared;
using LaDeak.JsonMergePatch.Shared;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, JsonOptions jsonOptions = null)
        {
            var typeRepository = new TypeRepository();
            typeRepository.TryAdd<WeatherForecast, WeatherForecastWrapper>();
            builder.Services.AddSingleton<ITypeRepository>(typeRepository);
            jsonOptions ??= new JsonOptions();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new JsonMergePatchInputReader(jsonOptions)));
        }
    }
}
