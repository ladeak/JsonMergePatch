using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    [Generator]
    public class JsonMergePatchSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var typeBuilder = new TypeBuilderGenerator(context.Compilation.SyntaxTrees, context.Compilation);
            var types = typeBuilder.Generate();
            foreach(var generatedType in types)
                context.AddSource(generatedType.FileName, SourceText.From(generatedType.SourceCode, Encoding.UTF8));
            var modelBuilderGenerator = new ModelBuilderExtensionGenerator();
            var mvcExtension = modelBuilderGenerator.CreateModelBuilder(types.Select(x => (x.SourceTypeFullName, x.TargetTypeFullName)));
            context.AddSource("LaDeakJsonMergePatchModelBuilderExtension", SourceText.From(mvcExtension, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
