using System.Collections.Generic;
using System.Text;
using LaDeak.JsonMergePatch.SourceGenerator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LaDeak.JsonMergePatch.SourceGenerator.AspNetCore
{
    [Generator]
    public class JsonMergePatchSourceGenerator : JsonMergePatchSourceGeneratorBase, ISourceGenerator
    {
        protected override IEnumerable<GeneratedWrapper> ExecuteImpl(GeneratorExecutionContext context)
        {
            var types = base.ExecuteImpl(context);
            var modelBuilderGenerator = new ModelBuilderExtensionGenerator();
            var mvcExtension = modelBuilderGenerator.CreateModelBuilder("LaDeak.JsonMergePatch.Generated.TypeRepositoryContainer.Instance.Repository");
            context.AddSource("LaDeakJsonMergePatchModelBuilderExtension", SourceText.From(mvcExtension, Encoding.UTF8));
            return types;
        }
    }
}
