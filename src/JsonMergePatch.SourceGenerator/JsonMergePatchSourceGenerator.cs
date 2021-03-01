using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    [Generator]
    public class JsonMergePatchSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context) => ExecuteImpl(context);

        protected virtual IEnumerable<GeneratedWrapper> ExecuteImpl(GeneratorExecutionContext context)
        {
            var typeBuilder = new MultiTypeBuilder(context.Compilation.SyntaxTrees, context.Compilation, new TypeBuilder(), new PatchParametersWalker());
            var types = typeBuilder.Generate();
            foreach (var generatedType in types)
                context.AddSource(generatedType.FileName, SourceText.From(generatedType.SourceCode, Encoding.UTF8));

            var tyeRepoGenerator = new TypeRepositoryGenerator();
            var typeRepoClass = tyeRepoGenerator.CreateTypeRepository(types.Select(x => (x.SourceTypeFullName, x.TargetTypeFullName)), context.Compilation.Assembly.Name);
            context.AddSource("LaDeakJsonMergePatchTypeRepo", SourceText.From(typeRepoClass, Encoding.UTF8));
            return types;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
