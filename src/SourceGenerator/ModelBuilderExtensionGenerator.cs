using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class ModelBuilderExtensionGenerator
    {
        public string CreateModelBuilder(string typeRepositoryAccessor)
        {
            if (string.IsNullOrWhiteSpace(typeRepositoryAccessor))
                return string.Empty;

            StringBuilder sb = new StringBuilder(@"
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch.Generated
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, JsonOptions jsonOptions = null)
        {
            builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>(");
            sb.AppendLine($"{typeRepositoryAccessor});");
            sb.AppendLine($"            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = {typeRepositoryAccessor};");
            sb.Append(@"            jsonOptions ??= new JsonOptions();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
        }
    }
}");
            return sb.ToString();
        }


    }
}
