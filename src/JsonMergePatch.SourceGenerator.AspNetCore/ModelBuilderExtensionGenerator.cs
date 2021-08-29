using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator.AspNetCore
{
    public class ModelBuilderExtensionGenerator
    {
        public string CreateModelBuilder(string typeRepositoryAccessor)
        {
            StringBuilder sb = new StringBuilder(@"
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch.Generated
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, JsonOptions jsonOptions = null, LaDeak.JsonMergePatch.Abstractions.ITypeRepository typeRepository = null)
        {
");
            if (!string.IsNullOrWhiteSpace(typeRepositoryAccessor))
            {
                sb.AppendLine($"            builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>(typeRepository ?? {typeRepositoryAccessor});");
                sb.AppendLine($"            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = typeRepository ?? {typeRepositoryAccessor};");
            }
            sb.Append(@"            jsonOptions ??= new JsonOptions();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
        }

        public static IMvcBuilder AddJsonMergePatch<T>(this IMvcBuilder builder, JsonOptions jsonOptions = null, LaDeak.JsonMergePatch.Abstractions.ITypeRepository typeRepository = null) where T : System.Text.Json.Serialization.JsonSerializerContext, new()
        {
");
            if (!string.IsNullOrWhiteSpace(typeRepositoryAccessor))
            {
                sb.AppendLine($"            builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>(typeRepository ?? {typeRepositoryAccessor});");
                sb.AppendLine($"            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = typeRepository ?? {typeRepositoryAccessor};");
            }
            sb.Append(@"            jsonOptions ??= new JsonOptions();
            jsonOptions.SerializerOptions.AddContext<T>();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
        }
    }
}");
            return sb.ToString();
        }


    }
}
