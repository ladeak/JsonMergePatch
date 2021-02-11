using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public class ModelBuilderExtensionGenerator
    {
        public string CreateModelBuilder(IEnumerable<(string, string)> typeRegistrations)
        {
            StringBuilder sb = new StringBuilder(@"
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LaDeak.JsonMergePatch.Generated
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddJsonMergePatch(this IMvcBuilder builder, JsonOptions jsonOptions = null)
        {
            var typeRepository = new TypeRepository();
");

            foreach ((var originalType, var generatedType) in typeRegistrations ?? Enumerable.Empty<(string, string)>())
                sb.AppendLine($"            typeRepository.Add<{originalType}, {generatedType}>();");

            sb.Append(@"            builder.Services.AddSingleton<ITypeRepository>(typeRepository);
            jsonOptions ??= new JsonOptions();
            return builder.AddMvcOptions(options => options.InputFormatters.Insert(0, new JsonMergePatchInputReader(jsonOptions)));
        }
    }
}");
            return sb.ToString();
        }


    }
}
