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
        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, JsonOptions jsonOptions = null)
        {");
            if (!string.IsNullOrWhiteSpace(typeRepositoryAccessor))
            {
                sb.AppendLine($"            builder.Services.AddSingleton<LaDeak.JsonMergePatch.Abstractions.ITypeRepository>({typeRepositoryAccessor});");
                sb.AppendLine($"            LaDeak.JsonMergePatch.Abstractions.JsonMergePatchOptions.Repository = {typeRepositoryAccessor};");
            }
            sb.Append(@"            jsonOptions ??= new JsonOptions();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new LaDeak.JsonMergePatch.AspNetCore.JsonMergePatchInputReader(jsonOptions)));
        }
    }
}");
            return sb.ToString();
        }


    }
}
