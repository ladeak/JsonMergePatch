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
            var modelBuilderGenerator = new ModelBuilderExtensionGenerator();
            var mvcExtension = modelBuilderGenerator.CreateModelBuilder("LaDeak.JsonMergePatch.Generated.TypeRepositoryContainer.Instance.Repository");
            context.AddSource("LaDeakJsonMergePatchModelBuilderExtension", SourceText.From(mvcExtension, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
