using System.Collections.Generic;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions
{
    public interface ITypeBuilderGenerator
    {
        IEnumerable<GeneratedWrapper> Generate();
    }
}