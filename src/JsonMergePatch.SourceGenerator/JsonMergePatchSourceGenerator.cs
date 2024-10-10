using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator;

[Generator]
public class JsonMergePatchSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName("LaDeak.JsonMergePatch.Abstractions.PatchableAttribute", (x, token) => true,
            static (GeneratorAttributeSyntaxContext context, CancellationToken token) =>
            {
                var typeSymbol = context.TargetSymbol as INamedTypeSymbol;
                if (!GeneratedTypeFilter.TryGetGeneratableType(typeSymbol, out var _))
                    return null;
                var assemblyName = context.SemanticModel.Compilation.Assembly.Name;
                var sourceTypeName = GeneratedTypeFilter.SourceTypeName(typeSymbol);
                return new GeneratorClassInfo(typeSymbol, sourceTypeName, assemblyName);
            }).Where(x => x != null);

        context.RegisterImplementationSourceOutput(provider, (spc, classInfo) =>
        {
            var builder = new TypeBuilder();
            var generatedType = builder.BuildWrapperType(classInfo);
            spc.AddSource(generatedType.FileName, SourceText.From(generatedType.SourceCode, Encoding.UTF8));
        });

        context.RegisterImplementationSourceOutput(provider.Collect(), (spc, classInfos) =>
        {
            var tyeRepoGenerator = new TypeRepositoryGenerator();
            var typeRepoClass = tyeRepoGenerator.CreateTypeRepository(classInfos.Select(x => (x.SourceTypeName, x.FullTypeName)), classInfos.First().AssemblyName);
            spc.AddSource("LaDeakJsonMergePatchTypeRepo", SourceText.From(typeRepoClass, Encoding.UTF8));
        });
    }
}
