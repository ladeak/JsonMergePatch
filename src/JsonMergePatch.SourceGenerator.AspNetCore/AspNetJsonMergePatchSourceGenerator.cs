using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator.AspNetCore
{
    [Generator]
    public class AspNetJsonMergePatchSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var currentAssemblyName = context.Compilation.Assembly.Name;
            var namespaceName = NameBuilder.GetNamespace(currentAssemblyName);
            var modelBuilderGenerator = new ModelBuilderExtensionGenerator();
            var mvcExtension = modelBuilderGenerator.CreateModelBuilder($"{namespaceName}.TypeRepository.Instance");
            context.AddSource("LaDeakJsonMergePatchModelBuilderExtension", SourceText.From(mvcExtension, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
